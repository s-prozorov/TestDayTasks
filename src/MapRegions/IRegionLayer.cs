namespace MapRegions;

public interface IRegionLayer
{
    int Width { get; }
    int Height { get; }
    int RegionCount { get; }
    
    ushort GetRegionId(int x, int y);
    RegionMetadata GetRegionMetadata(ushort regionId);
    bool TilebelongsToRegion(int x, int y, ushort regionId);
    IReadOnlySet<ushort> GetRegionsInArea(int x, int y, int width, int height);
}

