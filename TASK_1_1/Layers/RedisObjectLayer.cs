using StackExchange.Redis;
using System.Text.Json;
using TASK_1_1.Entities;
using TASK_1_1.Infrastructure;

namespace TASK_1_1.Layers;
public class RedisObjectLayer : IObjectLayer
{
    private readonly IConnectionMultiplexer redis;
    private readonly string geoKey;
    private readonly string hashKey;

    public RedisObjectLayer(IConnectionMultiplexer redis, string geoKey = "map:geo", string hashKey = "map:objects")
    {
        this.redis = redis;
        this.geoKey = geoKey;
        this.hashKey = hashKey;
    }

    public event Action<MapObject> ObjectAdded;
    public event Action<MapObject> ObjectUpdated;
    public event Action<MapObject> ObjectDeleted;

    public void AddObject(MapObject obj)
    {
        var db = redis.GetDatabase();
        db.GeoAdd(geoKey, obj.ToGeoCoordinates().longitude, obj.ToGeoCoordinates().latitude, obj.Id.ToString());
        db.HashSet(hashKey, obj.Id.ToString(), JsonSerializer.Serialize(obj));
        ObjectAdded?.Invoke(obj);
    }

    public void UpdateObject(MapObject obj)
    {
        var db = redis.GetDatabase();
        db.GeoAdd(geoKey, obj.ToGeoCoordinates().longitude, obj.ToGeoCoordinates().latitude, obj.Id.ToString());
        db.HashSet(hashKey, obj.Id.ToString(), JsonSerializer.Serialize(obj));
        ObjectUpdated?.Invoke(obj);
    }

    public void RemoveObject(ulong id)
    {
        var db = redis.GetDatabase();
        db.GeoRemove(geoKey, id.ToString());
        db.HashDelete(hashKey, id.ToString());
        ObjectDeleted?.Invoke(new MapObject { Id = id });
    }

    public MapObject? GetObjectById(ulong id)
    {
        var db = redis.GetDatabase();
        var data = db.HashGet(hashKey, id.ToString());
        if (data.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<MapObject>(data!);
    }

    public MapObject? GetObjectByCoordinate(int x, int y)
    {
        var db = redis.GetDatabase();
        // Небольшой радиус для поиска объекта в тайле
        var results = db.GeoRadius(geoKey, x, y, 0.5, GeoUnit.Meters);
        foreach (var id in results)
        {
            if (ulong.TryParse(id.ToString(), out ulong objId))
            {
                var obj = GetObjectById(objId);
                if (obj != null && obj.Contains(x, y))
                    return obj;
            }
        }
        return null;
    }

    public IEnumerable<MapObject> GetObjectsInArea(int startX, int startY, int endX, int endY)
    {
        var db = redis.GetDatabase();
        double centerX = (startX + endX) / 2.0;
        double centerY = (startY + endY) / 2.0;
        double radius = Math.Max(endX - startX, endY - startY) / 2.0;

        var results = db.GeoRadius(geoKey, centerX, centerY, radius, GeoUnit.Meters);
        foreach (var id in results)
        {
            if (ulong.TryParse(id.ToString(), out ulong objId))
            {
                var obj = GetObjectById(objId);
                if (obj != null && obj.Intersects(startX, startY, endX, endY))
                    yield return obj;
            }
        }
    }

}
