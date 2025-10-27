namespace MapSurface;

public sealed class TileProperties : ITileProperties
{
    private static readonly TileProperties PlainProperties = new(true);
    private static readonly TileProperties MountainProperties = new(false);

    public bool CanPlaceObject { get; }

    private TileProperties(bool canPlaceObject)
    {
        CanPlaceObject = canPlaceObject;
    }

    public static ITileProperties GetProperties(TileType tileType)
    {
        return tileType switch
        {
            TileType.Plain => PlainProperties,
            TileType.Mountain => MountainProperties,
            _ => throw new ArgumentException($"Unknown tile type: {tileType}", nameof(tileType))
        };
    }
}

