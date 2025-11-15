using MapSystem.Core.Common;

namespace MapSystem.Core.ObjectLayer;

public sealed class InMemoryObjectLayer : IObjectLayer
{
    private readonly int _width;
    private readonly int _height;
    private readonly Dictionary<uint, MapObject> _objects = new();
    private uint _nextId = 1;

    internal InMemoryObjectLayer(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width, nameof(width));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height, nameof(height));
        _width = width;
        _height = height;
    }

    public event Action<MapObject>? ObjectAdded;
    public event Action<MapObject>? ObjectUpdated;
    public event Action<uint>? ObjectDeleted;

    public Task<uint> AddAsync(int x, int y, int width, int height)
    {
        ValidateHelpers.ValidateArea(x, y, width, height);
        ValidateHelpers.ValidateAreaBounds(x, y, width, height, _width, _height);

        var id = _nextId++;
        var obj = new MapObject(id, x, y, width, height);
        _objects[id] = obj;

        ObjectAdded?.Invoke(obj);
        return Task.FromResult(id);
    }

    public Task<MapObject?> GetByIdAsync(uint id)
    {
        _objects.TryGetValue(id, out var obj);
        return Task.FromResult(obj);
    }

    public Task<MapObject?> GetAtPositionAsync(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x, nameof(x));
        ArgumentOutOfRangeException.ThrowIfNegative(y, nameof(y));

        foreach (var obj in _objects.Values)
        {
            if (ValidateHelpers.ContainsPoint(obj, x, y))
                return Task.FromResult<MapObject?>(obj);
        }

        return Task.FromResult<MapObject?>(null);
    }

    public Task<IReadOnlyCollection<MapObject>> GetInAreaAsync(int x, int y, int width, int height)
    {
        ValidateHelpers.ValidateArea(x, y, width, height);

        var results = new List<MapObject>();
        foreach (var obj in _objects.Values)
        {
            if (ValidateHelpers.Intersects(obj, x, y, width, height))
                results.Add(obj);
        }

        return Task.FromResult<IReadOnlyCollection<MapObject>>(results);
    }

    public Task<bool> UpdateAsync(uint id, int newX, int newY, int newWidth, int newHeight)
    {
        ValidateHelpers.ValidateArea(newX, newY, newWidth, newHeight);
        ValidateHelpers.ValidateAreaBounds(newX, newY, newWidth, newHeight, _width, _height);

        if (!_objects.ContainsKey(id))
            return Task.FromResult(false);

        var newObj = new MapObject(id, newX, newY, newWidth, newHeight);
        _objects[id] = newObj;

        ObjectUpdated?.Invoke(newObj);
        return Task.FromResult(true);
    }

    public Task<bool> IsInAreaAsync(uint objectId, int x, int y, int width, int height)
    {
        ValidateHelpers.ValidateArea(x, y, width, height);

        if (!_objects.TryGetValue(objectId, out var obj))
            return Task.FromResult(false);

        return Task.FromResult(ValidateHelpers.Intersects(obj, x, y, width, height));
    }

    public Task<bool> RemoveAsync(uint id)
    {
        if (!_objects.Remove(id))
            return Task.FromResult(false);

        ObjectDeleted?.Invoke(id);
        return Task.FromResult(true);
    }
}
