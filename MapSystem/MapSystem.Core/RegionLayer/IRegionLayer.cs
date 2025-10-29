namespace MapSystem.Core.RegionLayer;

public interface IRegionLayer
{
    int Width { get; }
    int Height { get; }
    ushort GetRegionId(int x, int y);
    Region GetRegion(ushort id);
    bool IsInRegion(int x, int y, ushort regionId);
    IReadOnlyCollection<Region> GetRegionsInArea(int x, int y, int width, int height);
}