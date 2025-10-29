using System.Diagnostics.CodeAnalysis;

namespace MapSystem.Core.RegionLayer;

public sealed class Region
{
    public ushort Id { get; }
    public string Name { get; }
    private readonly Dictionary<string, object> _metadata = new();

    internal Region(ushort id, string name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
    public void SetMetadata<T>(string key, T value) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        _metadata[key] = value;
    }

    public T? GetMetadata<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _metadata.TryGetValue(key, out var value) && value is T typed 
            ? typed 
            : default;
    }

    public bool TryGetMetadata<T>(string key, [NotNullWhen(true)] out T? value)
    {
        ArgumentNullException.ThrowIfNull(key);
    
        if (_metadata.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }
    
        value = default;
        return false;
    }

    public bool HasMetadata(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _metadata.ContainsKey(key);
    }

    public bool RemoveMetadata(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _metadata.Remove(key);
    }
}