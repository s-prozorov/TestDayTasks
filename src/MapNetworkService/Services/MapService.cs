using MagicOnion;
using MagicOnion.Server;
using MapNetworkService.Contracts;
using MapObjects;
using MapRegions;
using MapUnits.Core;

namespace MapNetworkService.Services;

public sealed class MapService : ServiceBase<IMapService>, IMapService
{
    private readonly IMapObjectLayer _objectLayer;
    private readonly IRegionLayer _regionLayer;
    private readonly IUnitMovementService _unitService;

    public MapService(IMapObjectLayer objectLayer, IRegionLayer regionLayer, IUnitMovementService unitService)
    {
        _objectLayer = objectLayer ?? throw new ArgumentNullException(nameof(objectLayer));
        _regionLayer = regionLayer ?? throw new ArgumentNullException(nameof(regionLayer));
        _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
    }

    public async UnaryResult<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequest request)
    {
        try
        {
            var objects = await _objectLayer.GetObjectsInAreaAsync(
                request.X, 
                request.Y, 
                request.Width, 
                request.Height);

            var dtos = objects.Select(obj => new MapObjectDto
            {
                Id = obj.Id,
                X = obj.X,
                Y = obj.Y,
                Width = obj.Width,
                Height = obj.Height
            }).ToArray();

            return new GetObjectsInAreaResponse { Objects = dtos };
        }
        catch (Exception ex)
        {
            throw new ReturnStatusException(
                Grpc.Core.StatusCode.InvalidArgument, 
                $"Error getting objects in area: {ex.Message}");
        }
    }

    public UnaryResult<GetRegionsInAreaResponse> GetRegionsInAreaAsync(GetRegionsInAreaRequest request)
    {
        try
        {
            var regionIds = _regionLayer.GetRegionsInArea(
                request.X, 
                request.Y, 
                request.Width, 
                request.Height);

            var dtos = regionIds.Select(id =>
            {
                var metadata = _regionLayer.GetRegionMetadata(id);
                return new RegionDto
                {
                    Id = metadata.Id,
                    Name = metadata.Name
                };
            }).ToArray();

            return UnaryResult.FromResult(new GetRegionsInAreaResponse { Regions = dtos });
        }
        catch (Exception ex)
        {
            throw new ReturnStatusException(
                Grpc.Core.StatusCode.InvalidArgument, 
                $"Error getting regions in area: {ex.Message}");
        }
    }

    public UnaryResult<GetUnitsInAreaResponse> GetUnitsInAreaAsync(GetObjectsInAreaRequest request)
    {
        try
        {
            var units = _unitService.GetUnitsInArea(request.X, request.Y, request.Width, request.Height);
            var movements = _unitService.GetMovementsInArea(request.X, request.Y, request.Width, request.Height);

            var unitDtos = new UnitDto[units.Count];
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                unitDtos[i] = new UnitDto { Id = u.Id, X = u.X, Y = u.Y };
            }

            var movementDtos = new MovementDto[movements.Count];
            for (int i = 0; i < movements.Count; i++)
            {
                var m = movements[i];
                movementDtos[i] = new MovementDto
                {
                    UnitId = m.UnitId,
                    StartX = m.StartX,
                    StartY = m.StartY,
                    TargetX = m.TargetX,
                    TargetY = m.TargetY,
                    Speed = m.Speed
                };
            }

            return UnaryResult.FromResult(new GetUnitsInAreaResponse { Units = unitDtos, Movements = movementDtos });
        }
        catch (Exception ex)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, $"Error getting units: {ex.Message}");
        }
    }

    public UnaryResult StartUnitMovementAsync(StartMovementRequest request)
    {
        try
        {
            _unitService.StartMovement(request.UnitId, request.TargetX, request.TargetY);
            return default;
        }
        catch (Exception ex)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, ex.Message);
        }
    }
}

