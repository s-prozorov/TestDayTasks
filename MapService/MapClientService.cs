using MagicOnion;
using MagicOnion.Server;
using MapLib;
using MapLib.Interfaces;
using MapService.Events;
using MapService.Extensions;
using MapService.Interfaces;
using MapService.Requests;
using MapService.Response;
using MapService.Response.Models;
using ObjectsLib;
using ObjectsLib.Interfaces;
using ObjectsLib.Models;

namespace MapService;

public class MapClientService : ServiceBase<IMapService>, IMapService
{
    private readonly IMap _map;
    private readonly IObjectStorage _objectStorage;
    private readonly IMapEventBroadcaster _broadcaster;

    public MapClientService(IMap map, 
        IObjectStorage objectStorage,
        IMapEventBroadcaster broadcaster)
    {
        _map = map;
        _objectStorage = objectStorage;
        _broadcaster = broadcaster;
    }
    
    public async UnaryResult<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequest request)
    {
        request.ValidateCoordinates();

        // Вычисляем центр прямоугольника
        double centerX = (request.X1.Value + request.X2.Value) / 2.0;
        double centerY = (request.Y1.Value + request.Y2.Value) / 2.0;
        
        // Вычисляем полуширину и полувысоту
        double halfWidth = (request.X2.Value - request.X1.Value) / 2.0;
        double halfHeight = (request.Y2.Value - request.Y1.Value) / 2.0;
        
        double radiusKm = CoordinatesConverter.RectangleToCircleRadiusKm(
            centerX, centerY, halfWidth, halfHeight);
        
        var objects = await _objectStorage.GetObjectsInZoneAsync(
            centerX, centerY, radiusKm);
        
        // Фильтруем объекты, оставляя только те что в прямоугольнике
        var filteredObjects = objects
            .Where(obj => obj.X >= request.X1 && obj.X <= request.X2 &&
                          obj.Y >= request.Y1 && obj.Y <= request.Y2)
            .Select(obj => new MapObject
            {
                Id = obj.Id,
                X = obj.X,
                Y = obj.Y,
                Type = (int)obj.Type,
                OwnerId = obj.OwnerId
            })
            .ToArray();

        return new GetObjectsInAreaResponse
        {
            Objects = filteredObjects
        };
    }

    public UnaryResult<GetRegionsInAreaResponse> GetRegionsInAreaAsync(GetRegionsInAreaRequest request)
    {
        request.ValidateCoordinates();
        
        const int FAST_COMPUTE_AREA_SIZE = 2500;
        
        var regions = new HashSet<int>();
        int width = request.X2.Value - request.X1.Value + 1;
        int height = request.Y2.Value - request.Y1.Value + 1;
        int area = width * height;

        if (area <= FAST_COMPUTE_AREA_SIZE) // 50x50
        {
            for (int x = request.X1.Value; x <= request.X2; x++)
            {
                for (int y = request.Y1.Value; y <= request.Y2; y++)
                {
                    regions.Add(_map.GetTerritoryId(x, y));
                }
            }
        }
        else
        {
            int step = Math.Max(1, width / 50);
            
            for (int x = request.X1.Value; x <= request.X2; x += step)
            {
                for (int y = request.Y1.Value; y <= request.Y2; y += step)
                {
                    regions.Add(_map.GetTerritoryId(x, y));
                }
            }
            
            // Границы для точности
            for (int x = request.X1.Value; x <= request.X2; x++)
            {
                regions.Add(_map.GetTerritoryId(x, request.Y1.Value));
                regions.Add(_map.GetTerritoryId(x, request.Y2.Value));
            }
            for (int y = request.Y1.Value; y <= request.Y2; y++)
            {
                regions.Add(_map.GetTerritoryId(request.X1.Value, y));
                regions.Add(_map.GetTerritoryId(request.X2.Value, y));
            }
        }

        var regionDtos = regions
            .Select(id =>
            {
                var territory = _map.GetTerritoryInfo(id);
                return new Region
                {
                    Id = territory.Id,
                    Name = territory.Name,
                    OwnerId = territory.OwnerId
                };
            })
            .ToArray();

        return UnaryResult.FromResult(new GetRegionsInAreaResponse { Regions = regionDtos });
    }
    
    public async UnaryResult<CreateObjectResponse> CreateObjectAsync(CreateObjectRequest request)
    {
        request.Validate();
        
        var objectData = new ObjectData
        {
            Id = Guid.NewGuid().ToString(),
            X = request.X.Value,
            Y = request.Y.Value,
            Type = (ObjectType)request.ObjectTypeId
        };

        var created = await _objectStorage.CreateObjectAsync(objectData);
        
        await _broadcaster.BroadcastObjectAddedAsync(new ObjectAddedEvent
        {
            Map = new MapObject
            {
                Id = created.Id,
                X = created.X,
                Y = created.Y,
                Type = (int)created.Type,
                OwnerId = created.OwnerId
            }
        });

        return new CreateObjectResponse { IsSuccess = true, ObjectId = created.Id };
    }

    public async UnaryResult<UpdateObjectResponse> UpdateObjectAsync(UpdateObjectRequest request)
    {
        request.Validate();
        // Проверяем существование
        var existing = await _objectStorage.GetObjectAsync(request.ObjectId);
        if (existing == null)
            throw new InvalidOperationException($"Object '{request.ObjectId}' not found");

        // Валидация координат
        if (request.X < 0 || request.X >= Map.Width || 
            request.Y < 0 || request.Y >= Map.Height)
            throw new ArgumentOutOfRangeException(nameof(request), 
                "Coordinates are out of map bounds");

        // Обновляем
        var updated = existing with 
        { 
            
            X = (double)request.X, 
            Y = (double)request.Y 
        };

        await _objectStorage.UpdateObjectAsync(updated);
        
        await _broadcaster.BroadcastObjectUpdatedAsync(new ObjectUpdatedEvent
        {
            Map = new MapObject
            {
                Id = updated.Id,
                X = updated.X,
                Y = updated.Y,
                Type = (int)updated.Type,
                OwnerId = updated.OwnerId
            }
        });

        return new UpdateObjectResponse { IsSuccess = true };
    }

    public async UnaryResult<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request)
    {
        request.Validate();
        
        var deleted = await _objectStorage.DeleteObjectAsync(request.ObjectId);

        if (deleted)
        {
            await _broadcaster.BroadcastObjectDeletedAsync(new ObjectDeletedEvent
            {
                ObjectId = request.ObjectId
            });
        }

        return new DeleteObjectResponse { IsSuccess = deleted };
    }
}