using System.Text.Json;
using ObjectsLib.Interfaces;
using ObjectsLib.Models;
using StackExchange.Redis;

namespace ObjectsLib;

public class ObjectStorage : IObjectStorage
{
    private readonly IDatabase _redis;
    private const string GeoIndexKey = "objects:geo";
    private const string DataKeyPrefix = "objects:data:";
    
    public ObjectStorage(IConnectionMultiplexer connection)
    {
        _redis = connection.GetDatabase();
    }
    
    public async Task<ObjectData> CreateObjectAsync(
        ObjectData objectData)
    {
        ArgumentNullException.ThrowIfNull(objectData);
    
        if (string.IsNullOrWhiteSpace(objectData.Id))
            throw new ArgumentException("Object ID cannot be empty", nameof(objectData));
    
        var dataKey = GetDataKey(objectData.Id);
        var json = JsonSerializer.Serialize(objectData);
    
        var transaction = _redis.CreateTransaction();
    
        // Добавляем condition как отдельную проверку
        transaction.AddCondition(Condition.KeyNotExists(dataKey));
    
        _ = transaction.StringSetAsync(dataKey, json);
        _ = transaction.GeoAddAsync(
            GeoIndexKey, 
            CoordinatesConverter.MapXToLongitude(objectData.X), 
            CoordinatesConverter.MapYToLatitude(objectData.Y), 
            objectData.Id);

        var committed = await transaction.ExecuteAsync();

        if (!committed)
            throw new InvalidOperationException(
                $"Object with ID '{objectData.Id}' already exists or transaction failed");

        return objectData;
    }
    
    public async Task<bool> DeleteObjectAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
    
        var dataKey = GetDataKey(id);
    
        var transaction = _redis.CreateTransaction();
    
        var geoRemoveTask = transaction.GeoRemoveAsync(GeoIndexKey, id);
        var dataRemoveTask = transaction.KeyDeleteAsync(dataKey);
    
        var committed = await transaction.ExecuteAsync();
    
        if (!committed)
            return false;
    
        var geoRemoved = await geoRemoveTask;
        var dataRemoved = await dataRemoveTask;
    
        return geoRemoved || dataRemoved;
    }
    
    public async Task<ObjectData> GetObjectAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        
        var dataKey = GetDataKey(id);
        var json = await _redis.StringGetAsync(dataKey);
        
        if (json.IsNullOrEmpty)
            return null;
        
        return JsonSerializer.Deserialize<ObjectData>(json);
    }

    public async Task<ObjectData> UpdateObjectAsync(ObjectData objectData)
    {
        ArgumentNullException.ThrowIfNull(objectData);

        if (string.IsNullOrWhiteSpace(objectData.Id))
            throw new ArgumentException("Object ID cannot be empty", nameof(objectData));

        var dataKey = GetDataKey(objectData.Id);

        var json = JsonSerializer.Serialize(objectData);

        var transaction = _redis.CreateTransaction();

        transaction.AddCondition(Condition.KeyExists(dataKey));

        _ = transaction.StringSetAsync(dataKey, json);

        _ = transaction.GeoAddAsync(
            GeoIndexKey,
            CoordinatesConverter.MapXToLongitude(objectData.X),
            CoordinatesConverter.MapYToLatitude(objectData.Y),
            objectData.Id);

        var committed = await transaction.ExecuteAsync();

        if (!committed)
            throw new InvalidOperationException($"Failed to update object '{objectData.Id}'");

        return objectData;
    }

    public async Task<IReadOnlyList<ObjectData>> GetObjectsInZoneAsync(
        double centerX, 
        double centerY, 
        double radiusKm)
    {
        if (radiusKm <= 0)
            throw new ArgumentOutOfRangeException(nameof(radiusKm), "Radius must be positive");
    
        var results = await _redis.GeoRadiusAsync(
            GeoIndexKey,
            CoordinatesConverter.MapXToLongitude(centerX),
            CoordinatesConverter.MapYToLatitude(centerY),
            radiusKm,
            GeoUnit.Kilometers);
        
        if (results.Length == 0)
            return Array.Empty<ObjectData>();
    
        // Формируем массив ключей
        var keys = results
            .Select(r => (RedisKey)GetDataKey(r.Member))
            .ToArray();
        
        var values = await _redis.StringGetAsync(keys);
    
        var objects = new List<ObjectData>(values.Length);
    
        foreach (var value in values)
        {
            if (!value.IsNullOrEmpty)
            {
                var objectData = JsonSerializer.Deserialize<ObjectData>(value);
                if (objectData != null)
                    objects.Add(objectData);
            }
        }
    
        return objects;
    }
    
    public async Task<bool> ExistsAsync(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        
        var dataKey = GetDataKey(id);
        return await _redis.KeyExistsAsync(dataKey);
    }
    
    private static string GetDataKey(string id) => $"{DataKeyPrefix}{id}";
}