namespace ObjectLayerEngine;

using StackExchange.Redis;
using System.Collections.Concurrent;

public class ObjectLayer : IDisposable
{
    private readonly IDatabase _redis;
    private readonly ISubscriber _pubSub;
    private readonly ConcurrentDictionary<string, MapObjectDTO> _localCache = new();
    public const string ObjectPrefix = ":obj";
    public const string GeoIndexKey = "objects:geo";
    public ConcurrentDictionary<string, MapObjectDTO> GetLocalCache() => _localCache;

    public ObjectLayer(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
        _pubSub = redis.GetSubscriber();
    }

    public async Task AddOrUpdateObjectAsync(MapObjectDTO obj)
    {
        var (lon, lat) = CoordinateConverter.ConvertToGeo(obj.X, obj.Y);
        var transaction = _redis.CreateTransaction();

        await transaction.GeoAddAsync(GeoIndexKey, new GeoEntry(lon, lat, obj.Id));
        await transaction.HashSetAsync($"{obj.Id}{ObjectPrefix}", new HashEntry[] 
        {
            new("x", obj.X),
            new("y", obj.Y),
            new("width", obj.Width),
            new("height", obj.Height)
        });
        if (await transaction.ExecuteAsync())
        {
            _localCache[obj.Id] = obj;
            await _pubSub.PublishAsync("objects:events", $"updated:{obj.Id}");
        }
    }
    
    public async Task<MapObjectDTO?> GetObjectAsync(string id)
    {
        if (_localCache.TryGetValue(id, out var cachedObj))
            return cachedObj;

        var data = await _redis.HashGetAllAsync($"{id}{ObjectPrefix}");
        if (!data.Any())
        {
            throw new KeyNotFoundException($"Object with id {id} not found");
        }
        
        return new MapObjectDTO
        {
            Id = id,
            X = (int)data[0].Value,
            Y = (int)data[1].Value,
            Width = (int)data[2].Value,
            Height = (int)data[3].Value
        };
    }

    public async Task<bool> RemoveObjectAsync(string id)
    {
        var obj = await GetObjectAsync(id);
        if (obj == null) return false;

        var transaction = _redis.CreateTransaction();
        await transaction.KeyDeleteAsync($"{id}{ObjectPrefix}");
        await transaction.GeoRemoveAsync(GeoIndexKey, id);

        if (!await transaction.ExecuteAsync()) return false;
        _localCache.TryRemove(id, out _);
        await _pubSub.PublishAsync("objects:events", $"removed:{id}");
        return true;
    }

    public async Task<MapObjectDTO> FindObjectAtAsync(int x, int y)
    {
        var (lon, lat) = CoordinateConverter.ConvertToGeo(x, y);
        var results = await _redis.GeoRadiusAsync(
            GeoIndexKey,
            lon,
            lat,
            1,
            GeoUnit.Meters,
            1,
            Order.Ascending,
            GeoRadiusOptions.WithCoordinates);

        foreach (var result in results)
        {
            var obj = await GetObjectAsync(result.Member.ToString());
            if (obj != null && IsPointInside(obj, x, y))
                return obj;
        }
        return null;
    }

    public async Task<IEnumerable<MapObjectDTO>> GetObjectsInAreaAsync(int x1, int y1, int x2, int y2)
    {
        var (minLon, minLat) = CoordinateConverter.ConvertToGeo(x1, y1);
        var (maxLon, maxLat) = CoordinateConverter.ConvertToGeo(x2, y2);

        var allElements = await _redis.GeoRadiusAsync(
            GeoIndexKey,
            0, 0,
            10000000,
            GeoUnit.Kilometers);

        // Фильтруем по прямоугольной области
        var filteredElements = allElements
            .Where(e => 
                e.Position.Value.Longitude >= minLon && 
                e.Position.Value.Longitude <= maxLon &&
                e.Position.Value.Latitude >= minLat &&
                e.Position.Value.Latitude <= maxLat)
            .ToList();

        var objects = new List<MapObjectDTO>();
        foreach (var member in filteredElements)
        {
            var obj = await GetObjectAsync(member.Member.ToString());
            if (obj != null && CheckObjectInArea(obj, x1, y1, x2, y2))
                objects.Add(obj);
        }
        return objects;
    }

    private bool IsPointInside(MapObjectDTO obj, int x, int y)
    {
        return x >= obj.X && x < obj.X + obj.Width &&
               y >= obj.Y && y < obj.Y + obj.Height;
    }

    private bool CheckObjectInArea(MapObjectDTO obj, int areaX1, int areaY1, int areaX2, int areaY2)
    {
        int objX2 = obj.X + obj.Width;
        int objY2 = obj.Y + obj.Height;

        return obj.X < areaX2 && objX2 > areaX1 &&
               obj.Y < areaY2 && objY2 > areaY1;
    }

    public void SubscribeToEvents(Action<string> handler)
    {
        _pubSub.Subscribe("objects:events", (channel, message) =>
        {
            handler(message.ToString());
        });
    }

    public void Dispose()
    {
        _pubSub.UnsubscribeAll();
    }
}