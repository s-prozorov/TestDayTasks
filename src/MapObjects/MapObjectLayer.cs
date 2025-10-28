using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MapObjects;

public sealed class MapObjectLayer : IMapObjectLayer
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly string _keyPrefix;
    private readonly string _geoKey;
    private readonly ConcurrentBag<Action<ObjectEvent>> _subscribers;

    public MapObjectLayer(string connectionString, string keyPrefix = "map:objects")
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        _redis = ConnectionMultiplexer.Connect(connectionString);
        _database = _redis.GetDatabase();
        _keyPrefix = keyPrefix;
        _geoKey = $"{_keyPrefix}:geo";
        _subscribers = new ConcurrentBag<Action<ObjectEvent>>();
    }

    public MapObjectLayer(IConnectionMultiplexer redis, string keyPrefix = "map:objects")
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _database = _redis.GetDatabase();
        _keyPrefix = keyPrefix;
        _geoKey = $"{_keyPrefix}:geo";
        _subscribers = new ConcurrentBag<Action<ObjectEvent>>();
    }

    public async Task AddObjectAsync(MapObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var (latitude, longitude) = CoordinateConverter.ToGeoCoordinates(obj.X, obj.Y);

        var transaction = _database.CreateTransaction();
        
        var objectKey = GetObjectKey(obj.Id);
        var objectData = SerializeObject(obj);
        
        _ = transaction.StringSetAsync(objectKey, objectData);
        _ = transaction.GeoAddAsync(_geoKey, longitude, latitude, obj.Id);
        
        bool committed = await transaction.ExecuteAsync();
        
        if (committed)
        {
            NotifySubscribers(new ObjectEvent(ObjectEventType.Added, obj));
        }
    }

    public async Task<MapObject?> GetObjectByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Object ID cannot be null or empty", nameof(id));

        var objectKey = GetObjectKey(id);
        var objectData = await _database.StringGetAsync(objectKey);

        if (!objectData.HasValue)
            return null;

        return DeserializeObject(objectData!);
    }

    public async Task<bool> RemoveObjectAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Object ID cannot be null or empty", nameof(id));

        var obj = await GetObjectByIdAsync(id);
        if (obj == null)
            return false;

        var objectKey = GetObjectKey(id);

        var transaction = _database.CreateTransaction();
        _ = transaction.KeyDeleteAsync(objectKey);
        _ = transaction.GeoRemoveAsync(_geoKey, id);

        bool committed = await transaction.ExecuteAsync();

        if (committed)
        {
            NotifySubscribers(new ObjectEvent(ObjectEventType.Deleted, obj));
        }

        return committed;
    }

    public async Task<MapObject?> GetObjectAtPositionAsync(int x, int y)
    {
        var objects = await GetObjectsInAreaAsync(x, y, 1, 1);
        return objects.FirstOrDefault(obj => obj.ContainsPoint(x, y));
    }

    public async Task<IReadOnlyList<MapObject>> GetObjectsInAreaAsync(int x, int y, int width, int height)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));

        var centerX = x + width / 2.0;
        var centerY = y + height / 2.0;
        var (centerLat, centerLon) = CoordinateConverter.ToGeoCoordinates((int)centerX, (int)centerY);

        var diagonal = Math.Sqrt(width * width + height * height);
        var maxObjectSize = 100;
        var searchRadius = (diagonal / 2.0 + maxObjectSize) * CoordinateConverter.DegreesPerTile * 111000;

        var results = await _database.GeoRadiusAsync(
            _geoKey,
            centerLon,
            centerLat,
            searchRadius,
            GeoUnit.Meters);

        var objects = new List<MapObject>();
        foreach (var result in results)
        {
            var obj = await GetObjectByIdAsync(result.Member.ToString());
            if (obj != null && obj.IntersectsArea(x, y, width, height))
            {
                objects.Add(obj);
            }
        }

        return objects;
    }

    public void Subscribe(Action<ObjectEvent> handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        _subscribers.Add(handler);
    }

    public void Unsubscribe(Action<ObjectEvent> handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var newSubscribers = _subscribers.Where(s => s != handler).ToList();
        _subscribers.Clear();
        foreach (var subscriber in newSubscribers)
        {
            _subscribers.Add(subscriber);
        }
    }

    public void Dispose()
    {
        _redis?.Dispose();
    }

    private string GetObjectKey(string id) => $"{_keyPrefix}:data:{id}";

    private string SerializeObject(MapObject obj)
    {
        return JsonSerializer.Serialize(new
        {
            obj.Id,
            obj.X,
            obj.Y,
            obj.Width,
            obj.Height
        });
    }

    private MapObject DeserializeObject(string data)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(data);
        return new MapObject(
            doc.GetProperty("Id").GetString()!,
            doc.GetProperty("X").GetInt32(),
            doc.GetProperty("Y").GetInt32(),
            doc.GetProperty("Width").GetInt32(),
            doc.GetProperty("Height").GetInt32()
        );
    }

    private void NotifySubscribers(ObjectEvent evt)
    {
        foreach (var subscriber in _subscribers)
        {
            try
            {
                subscriber(evt);
            }
            catch
            {
            }
        }
    }
}

