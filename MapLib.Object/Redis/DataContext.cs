using System.Text.Json;
using MapLib.Core.Models.ObjectModel;
using StackExchange.Redis;

namespace MapLib.Object.Redis;

public class DataContext
{
    private readonly IConnectionMultiplexer redis;
    private readonly IDatabase db;
    
    private const string ObjectsKeyPrefix = "objects:";
    private const string GeoIndexKey = "geo:objects";
    
    private const int CoordinateScale = 10000;
    
    public DataContext(IConnectionMultiplexer redis)
    {
        this.redis = redis;
        db = redis.GetDatabase();
    }

    public async Task<string> CreateAsync(ObjectInfo objectInfo)
    {
        if (string.IsNullOrEmpty(objectInfo.Id))
        {
            objectInfo.Id = Guid.NewGuid().ToString();
        }

        var key = GetObjectKey(objectInfo.Id);
        var json = JsonSerializer.Serialize(objectInfo);
        
        var trans = db.CreateTransaction();

        var set = trans.StringSetAsync(key, json);

        var geo = trans.GeoAddAsync(
            GeoIndexKey,
            NormalizeToLongitude(objectInfo.X),
            NormalizeToLatitude(objectInfo.Y),
            objectInfo.Id
        );

        await trans.ExecuteAsync();

        return objectInfo.Id;
    }
    
    public async Task<bool> Delete(string objectId)
    {
        var key = GetObjectKey(objectId);
        
        var exists = await db.KeyExistsAsync(key);
        if (!exists) return false;
        
        var json = await db.StringGetAsync(key);
        if (string.IsNullOrEmpty(json)) return false;
        
        var obj = JsonSerializer.Deserialize<ObjectInfo>(json);
        if (obj == null) return false;
        
        var trans = db.CreateTransaction();
        
        var del = trans.KeyDeleteAsync(key);
        var geoDel = trans.GeoRemoveAsync(GeoIndexKey, objectId);
        var typeDel = trans.SetRemoveAsync( 
            $"objects:index:type:{obj.Type}", 
            objectId);

        await trans.ExecuteAsync();

        return true;
    }

    public async Task<List<ObjectInfo>> GetObjectInZone(int x, int y, int rad)
    {
        var searchRad = rad / 100.0; 
        
        var results = await db.GeoRadiusAsync(
            GeoIndexKey,
            NormalizeToLongitude(x),
            NormalizeToLatitude(y),
            searchRad,
            GeoUnit.Kilometers);
        
        var objects = new List<ObjectInfo>();
        foreach (var result in results)
        {
            var obj = await GetByIdAsync(result.Member!);
            if (obj != null)
                objects.Add(obj);
        }

        return objects;
    }

    public async Task<List<ObjectInfo>> GetObjectsInAreaAsync(int x1, int x2, int y1, int y2)
    {
        var centerX = (x1 + x2) / 2.0;
        var centerY = (y1 + y2) / 2.0;
        var width = Math.Abs(x2 - x1);
        var height = Math.Abs(y2 - y1);
        var radius = Math.Max(width, height) / 2.0;
        
        var results = await db.GeoRadiusAsync(
            GeoIndexKey,
            NormalizeToLongitude((int)centerX),
            NormalizeToLatitude((int)centerY),
            radius / 100.0,
            GeoUnit.Kilometers);
        
        var objects = new List<ObjectInfo>();
        foreach (var result in results)
        {
            var obj = await GetByIdAsync(result.Member!);
            if (obj != null && IsInBounds(obj, x1, y1, x2, y2))
                objects.Add(obj);
        }
        
        return objects;
    }
    
    private bool IsInBounds(ObjectInfo obj, int x1, int y1, int x2, int y2)
    {
        int minX = Math.Min(x1, x2);
        int maxX = Math.Max(x1, x2);
        int minY = Math.Min(y1, y2);
        int maxY = Math.Max(y1, y2);
        
        return obj.X >= minX && obj.X <= maxX && obj.Y >= minY && obj.Y <= maxY;
    }

    public async Task<ObjectInfo?> GetByIdAsync(string objectId)
    {
        var key = GetObjectKey(objectId);
        var json = await db.StringGetAsync(key);
        
        if (json.IsNullOrEmpty)
            return null;
        
        return JsonSerializer.Deserialize<ObjectInfo>(json!);
    }

    private string GetObjectKey(string objectId) => $"{ObjectsKeyPrefix}{objectId}";
    
    private double NormalizeToLongitude(int x) => (x - 500) / (double)CoordinateScale;
    private double NormalizeToLatitude(int y) => (y - 500) / (double)CoordinateScale;
}