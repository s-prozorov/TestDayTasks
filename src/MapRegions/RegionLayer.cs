namespace MapRegions;

public sealed class RegionLayer : IRegionLayer
{
    private readonly ushort[] _regions;
    private readonly Dictionary<ushort, RegionMetadata> _metadata;
    private readonly int _width;
    private readonly int _height;

    public int Width => _width;
    public int Height => _height;
    public int RegionCount => _metadata.Count;

    private RegionLayer(int width, int height, ushort[] regions, Dictionary<ushort, RegionMetadata> metadata)
    {
        _width = width;
        _height = height;
        _regions = regions;
        _metadata = metadata;
    }

    public static RegionLayer Create(int width, int height, int regionCount, IRegionGenerator generator, Func<ushort, string> nameGenerator)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));
        if (regionCount <= 0)
            throw new ArgumentException("Region count must be positive", nameof(regionCount));
        if (generator == null)
            throw new ArgumentNullException(nameof(generator));
        if (nameGenerator == null)
            throw new ArgumentNullException(nameof(nameGenerator));

        var regions = generator.Generate(width, height, regionCount);
        var metadata = new Dictionary<ushort, RegionMetadata>();

        for (ushort i = 0; i < regionCount; i++)
        {
            metadata[i] = new RegionMetadata(i, nameGenerator(i));
        }

        return new RegionLayer(width, height, regions, metadata);
    }

    public ushort GetRegionId(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _regions[y * _width + x];
    }

    public RegionMetadata GetRegionMetadata(ushort regionId)
    {
        if (!_metadata.TryGetValue(regionId, out var metadata))
            throw new ArgumentException($"Region with ID {regionId} does not exist", nameof(regionId));

        return metadata;
    }

    public bool TilebelongsToRegion(int x, int y, ushort regionId)
    {
        ValidateCoordinates(x, y);
        return _regions[y * _width + x] == regionId;
    }

    public IReadOnlySet<ushort> GetRegionsInArea(int x, int y, int width, int height)
    {
        ValidateArea(x, y, width, height);

        var regions = new HashSet<ushort>();

        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                ushort regionId = _regions[(y + dy) * _width + (x + dx)];
                regions.Add(regionId);
            }
        }

        return regions;
    }

    private void ValidateCoordinates(int x, int y)
    {
        if (x < 0 || x >= _width)
            throw new ArgumentOutOfRangeException(nameof(x), $"X coordinate must be between 0 and {_width - 1}");
        if (y < 0 || y >= _height)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y coordinate must be between 0 and {_height - 1}");
    }

    private void ValidateArea(int x, int y, int width, int height)
    {
        if (x < 0 || x >= _width)
            throw new ArgumentOutOfRangeException(nameof(x), $"X coordinate must be between 0 and {_width - 1}");
        if (y < 0 || y >= _height)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y coordinate must be between 0 and {_height - 1}");
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));
        if (x + width > _width)
            throw new ArgumentOutOfRangeException(nameof(width), $"Area exceeds map bounds: x + width = {x + width}, map width = {_width}");
        if (y + height > _height)
            throw new ArgumentOutOfRangeException(nameof(height), $"Area exceeds map bounds: y + height = {y + height}, map height = {_height}");
    }
}

