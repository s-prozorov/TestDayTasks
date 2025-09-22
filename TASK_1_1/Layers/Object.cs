using TASK_1_1.Infrastructure;

namespace TASK_1_1.Layers;
public class ObjectLayer : IObjectLayer
{
    private readonly Dictionary<string, MapObject> objects = new();
    private readonly IRedisGeoClient redis;
    private const string RedisKey = "map_objects";

    public event Action<MapObject>? ObjectAdded;
    public event Action<MapObject>? ObjectRemoved;
    public event Action<MapObject>? ObjectUpdated;

    public ObjectLayer(IRedisGeoClient redisClient)
    {
        redis = redisClient;
    }

    public void AddObject(MapObject obj)
    {
        objects[obj.Id] = obj;
        var (lat, lon) = CoordinateConverter.ToGeo(obj.X, obj.Y);
        redis.GeoAdd(RedisKey, lon, lat, obj.Id);
        ObjectAdded?.Invoke(obj);
    }

    public void RemoveObject(string id)
    {
        if (objects.TryGetValue(id, out var obj))
        {
            objects.Remove(id);
            redis.GeoRemove(RedisKey, id);
            ObjectRemoved?.Invoke(obj);
        }
    }

    public MapObject? GetObjectById(string id) =>
        objects.TryGetValue(id, out var obj) ? obj : null;

    public MapObject? GetObjectAt(int x, int y) =>
        objects.Values.FirstOrDefault(o => o.Contains(x, y));

    public IEnumerable<MapObject> GetObjectsInArea(int startX, int startY, int endX, int endY)
    {
        double centerX = (startX + endX) / 2.0;
        double centerY = (startY + endY) / 2.0;
        double radius = Math.Max(endX - startX, endY - startY) / 2.0;

        var (lat, lon) = CoordinateConverter.ToGeo((int)centerX, (int)centerY);

        var ids = redis.GeoRadius(RedisKey, lon, lat, radius);
        return ids.Select(id => GetObjectById(id)!).Where(o => o.Intersects(startX, startY, endX, endY));
    }

    public void UpdateObject(MapObject obj)
    {
        if (!objects.ContainsKey(obj.Id))
            throw new KeyNotFoundException($"Object '{obj.Id}' not found.");

        objects[obj.Id] = obj;

        var (lat, lon) = CoordinateConverter.ToGeo(obj.X, obj.Y);
        redis.GeoAdd("map_objects", lon, lat, obj.Id); // обновляем координаты в Redis

        ObjectUpdated?.Invoke(obj);
    }

}
public static class CoordinateConverter
{
    private const double BaseLat = 0.0;
    private const double BaseLon = 0.0;
    private const double TileSize = 0.0001;

    public static (double lat, double lon) ToGeo(int x, int y)
    {
        double lat = BaseLat + y * TileSize;
        double lon = BaseLon + x * TileSize;
        return (lat, lon);
    }
}
