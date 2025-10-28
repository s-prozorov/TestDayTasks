namespace MapRegions;

public sealed class RegionMetadata
{
    public ushort Id { get; }
    public string Name { get; }

    public RegionMetadata(ushort id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Region name cannot be null or empty", nameof(name));

        Id = id;
        Name = name;
    }
}

