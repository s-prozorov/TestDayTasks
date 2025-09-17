using ObjectLayerEngine;
using StackExchange.Redis;

public class ObjectLayer : IDisposable
{
    private readonly ConnectionMultiplexer redis;
    private readonly IDictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
    
    public ObjectLayer(string redisConnectionString)
    {
        redis = ConnectionMultiplexer.Connect(redisConnectionString);
    }

    private (double, double) ConvertCoordinates(int x, int y)
    {
        // Normalize coordinates to -180..180 and -90..90
        return (x / 1000.0 * 360 - 180, y / 1000.0 * 180 - 90);
    }

    public void AddObject(GameObject obj)
    {
        var (lon, lat) = ConvertCoordinates(obj.X, obj.Y);
        var db = redis.GetDatabase();
        db.GeoAdd("objects", new GeoEntry(lon, lat, obj.Id));
        objects[obj.Id] = obj;
    }

    public GameObject GetById(string id) => objects.TryGetValue(id, out var obj) ? obj : default;

    public void RemoveObject(string id)
    {
        if (objects.Remove(id, out var obj))
        {
            var db = redis.GetDatabase();
            db.GeoRemove("objects", id);
        }
    }

    public IEnumerable<GameObject> GetObjectsInArea(int x, int y, int radius)
    {
        var (lon, lat) = ConvertCoordinates(x, y);
        var db = redis.GetDatabase();
        var results = db.GeoRadius("objects", lon, lat, radius, GeoUnit.Meters);
        
        return results.Select(entry => GetById(entry.Member.ToString()));
    }

    public void Dispose() => redis?.Dispose();
}