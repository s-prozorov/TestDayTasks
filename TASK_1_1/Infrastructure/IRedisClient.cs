using TASK_1_1.Entities;

namespace TASK_1_1.Infrastructure;
public interface IRedisGeoClient
{
    int Width { get; }
    int Height { get; }

    ushort GetRegionId(int x, int y);
    Region GetRegionById(ushort regionId);
    bool IsTileInRegion(int x, int y, ushort regionId);
    IEnumerable<Region> GetRegionsInArea(int startX, int startY, int endX, int endY);

    void GenerateRegions(IEnumerable<Region> regions);
}
