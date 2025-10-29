using MapSystem.Core.Common;
using StackExchange.Redis;

namespace MapSystem.Core.ObjectLayer;

public sealed class RedisObjectLayer : IObjectLayer, IDisposable
{
    private readonly IDatabase _db;
    private readonly string _geoKey;
    private readonly string _idCounterKey;
    private readonly string _keyPrefix;
    private readonly IConnectionMultiplexer _redis;

    internal RedisObjectLayer(IConnectionMultiplexer redis, string keyPrefix)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _db = redis.GetDatabase();
        _keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));

        _geoKey = $"{_keyPrefix}:geo";
        _idCounterKey = $"{_keyPrefix}:next_id";
    }

    public void Dispose()
    {
        _redis?.Dispose();
    }

    public event Action<MapObject>? ObjectAdded;
    public event Action<MapObject>? ObjectUpdated;
    public event Action<uint>? ObjectDeleted;

    public async Task<uint> AddAsync(int x, int y, int width, int height)
    {
        ValidateHelpers.ValidateArea(x, y, width, height);

        var nextId = await _db.StringIncrementAsync(_idCounterKey);

        if (nextId > uint.MaxValue)
            throw new InvalidOperationException($"Превышен лимит ID объектов: {uint.MaxValue}");

        var id = (uint)nextId;
        var obj = new MapObject(id, x, y, width, height);

        var (centerX, centerY) = ValidateHelpers.GetCenter(obj);
        var (latitude, longitude) = CoordinateConverter.ToGeoCoordinates(
            (int)Math.Round(centerX),
            (int)Math.Round(centerY));

        var hashEntries = new[]
        {
            new HashEntry("id", id),
            new HashEntry("x", x),
            new HashEntry("y", y),
            new HashEntry("width", width),
            new HashEntry("height", height)
        };

        var objectKey = GetObjectKey(id);

        var transaction = _db.CreateTransaction();
        var geoAddTask = transaction.GeoAddAsync(_geoKey, longitude, latitude, id.ToString());
        var hashSetTask = transaction.HashSetAsync(objectKey, hashEntries);

        var committed = await transaction.ExecuteAsync();

        if (!committed)
            throw new InvalidOperationException("Не удалось добавить объект в Redis");

        ObjectAdded?.Invoke(obj);

        return id;
    }

    public async Task<MapObject?> GetByIdAsync(uint id)
    {
        var objectKey = GetObjectKey(id);
        var hashEntries = await _db.HashGetAllAsync(objectKey);

        if (hashEntries.Length == 0)
            return null;

        return ParseMapObject(hashEntries);
    }

    public async Task<MapObject?> GetAtPositionAsync(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x, nameof(x));
        ArgumentOutOfRangeException.ThrowIfNegative(y, nameof(y));

        var candidates = await GetInAreaAsync(x, y, 1, 1);

        foreach (var obj in candidates)
            if (ValidateHelpers.ContainsPoint(obj, x, y))
                return obj;

        return null;
    }

    public async Task<IReadOnlyCollection<MapObject>> GetInAreaAsync(int x, int y, int width, int height)
    {
        ValidateHelpers.ValidateArea(x, y, width, height);

        var centerX = x + width / 2.0;
        var centerY = y + height / 2.0;

        var (latitude, longitude) = CoordinateConverter.ToGeoCoordinates(
            (int)Math.Round(centerX),
            (int)Math.Round(centerY));

        var radiusKm = CoordinateConverter.CalculateSearchRadiusKm(width, height);

        var geoResults = await _db.GeoRadiusAsync(
            _geoKey,
            longitude,
            latitude,
            radiusKm,
            GeoUnit.Kilometers,
            -1,
            Order.Ascending);

        if (geoResults.Length == 0)
            return Array.Empty<MapObject>();

        var results = new List<MapObject>(geoResults.Length);

        foreach (var geoResult in geoResults)
        {
            if (!uint.TryParse(geoResult.Member, out var candidateId))
                continue;

            var obj = await GetByIdAsync(candidateId);
            if (obj == null)
                continue;

            if (ValidateHelpers.Intersects(obj, x, y, width, height)) results.Add(obj);
        }

        return results.ToArray();
    }

    public async Task<bool> UpdateAsync(
        uint id,
        int newX,
        int newY,
        int newWidth,
        int newHeight)
    {
        ValidateHelpers.ValidateArea(newX, newY, newWidth, newHeight);

        var oldObj = await GetByIdAsync(id);
        if (oldObj == null)
            return false;

        var newObj = new MapObject(id, newX, newY, newWidth, newHeight);
        var (centerX, centerY) = ValidateHelpers.GetCenter(newObj);
        var (latitude, longitude) = CoordinateConverter.ToGeoCoordinates(
            (int)Math.Round(centerX),
            (int)Math.Round(centerY));

        var hashEntries = new[]
        {
            new HashEntry("id", id),
            new HashEntry("x", newX),
            new HashEntry("y", newY),
            new HashEntry("width", newWidth),
            new HashEntry("height", newHeight)
        };

        var objectKey = GetObjectKey(id);

        var transaction = _db.CreateTransaction();
        var geoRemoveTask = transaction.GeoRemoveAsync(_geoKey, id.ToString());
        var geoAddTask = transaction.GeoAddAsync(_geoKey, longitude, latitude, id.ToString());
        var hashSetTask = transaction.HashSetAsync(objectKey, hashEntries);

        var committed = await transaction.ExecuteAsync();

        if (!committed)
            throw new InvalidOperationException("Не удалось обновить объект в Redis");

        ObjectUpdated?.Invoke(newObj);

        return true;
    }

    public async Task<bool> IsInAreaAsync(uint objectId, int x, int y, int width, int height)
    {
        ValidateHelpers.ValidateArea(x, y, width, height);

        var obj = await GetByIdAsync(objectId);
        if (obj == null)
            return false;

        return ValidateHelpers.Intersects(obj, x, y, width, height);
    }

    public async Task<bool> RemoveAsync(uint id)
    {
        var objectKey = GetObjectKey(id);

        var exists = await _db.KeyExistsAsync(objectKey);
        if (!exists)
            return false;

        await _db.GeoRemoveAsync(_geoKey, id.ToString());
        await _db.KeyDeleteAsync(objectKey);

        ObjectDeleted?.Invoke(id);

        return true;
    }

    public static async Task<RedisObjectLayer> CreateAsync(
        string connectionString,
        string keyPrefix = "map")
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string не может быть пустым", nameof(connectionString));

        if (string.IsNullOrWhiteSpace(keyPrefix))
            throw new ArgumentException("Key prefix не может быть пустым", nameof(keyPrefix));

        var redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        var db = redis.GetDatabase();
        await db.PingAsync();

        return new RedisObjectLayer(redis, keyPrefix);
    }

    private string GetObjectKey(uint id)
    {
        return $"{_keyPrefix}:obj:{id}";
    }

    private static MapObject ParseMapObject(HashEntry[] hashEntries)
    {
        var hash = hashEntries.ToDictionary(
            entry => entry.Name.ToString(),
            entry => entry.Value);

        if (!hash.TryGetValue("id", out var idValue) ||
            !hash.TryGetValue("x", out var xValue) ||
            !hash.TryGetValue("y", out var yValue) ||
            !hash.TryGetValue("width", out var widthValue) ||
            !hash.TryGetValue("height", out var heightValue))
            throw new InvalidOperationException("Hash объекта в Redis содержит неполные данные");

        var id = (uint)(int)idValue;
        var x = (int)xValue;
        var y = (int)yValue;
        var width = (int)widthValue;
        var height = (int)heightValue;

        return new MapObject(id, x, y, width, height);
    }
}