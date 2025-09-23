using TASK_1_1.Entities;

namespace TASK_1_1.Infrastructure;

public interface IRegionLayer
{
    int Width { get; }
    int Height { get; }

    ushort GetRegionId(int x, int y);

    Region GetRegionById(ushort id);

    bool IsTileInRegion(int x, int y, ushort id);

    IEnumerable<Region> GetRegionsInArea(int startX, int startY, int endX, int endY);

    void GenerateRegions(IEnumerable<Region> regions);

    void Resize(int newWidth, int newHeight);
}