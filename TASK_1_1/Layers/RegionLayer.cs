using TASK_1_1.Entities;
using TASK_1_1.Infrastructure;

namespace TASK_1_1.Layers;
public class RegionLayer : IRegionLayer
{
    private ushort[] mapRegionIds;
    private readonly Dictionary<ushort, Region> regionsById = new();
    public int Width { get; private set; }
    public int Height { get; private set; }

    public RegionLayer(int width, int height)
    {
        Width = width;
        Height = height;
        mapRegionIds = new ushort[width * height];
    }

    private int Index(int x, int y) => y * Width + x;

    public void GenerateRegions(IEnumerable<Region> regions)
    {
        regionsById.Clear();
        foreach (var region in regions)
        {
            regionsById[region.Id] = region;
            foreach (var tile in region.Tiles)
            {
                mapRegionIds[Index(tile.X, tile.Y)] = region.Id;
            }
        }
    }

    public ushort GetRegionId(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException();
        return mapRegionIds[Index(x, y)];
    }

    public Region GetRegionById(ushort id)
    {
        if (!regionsById.TryGetValue(id, out var region))
            throw new KeyNotFoundException();
        return region;
    }

    public bool IsTileInRegion(int x, int y, ushort id) => GetRegionId(x, y) == id;

    public IEnumerable<Region> GetRegionsInArea(int startX, int startY, int endX, int endY)
    {
        var found = new HashSet<ushort>();
        for (int y = Math.Max(0, startY); y <= Math.Min(Height - 1, endY); y++)
            for (int x = Math.Max(0, startX); x <= Math.Min(Width - 1, endX); x++)
                found.Add(mapRegionIds[Index(x, y)]);

        return found.Select(id => regionsById[id]);
    }

    public void Resize(int newWidth, int newHeight)
    {
        var newMap = new ushort[newWidth * newHeight];
        for (int y = 0; y < Math.Min(Height, newHeight); y++)
            for (int x = 0; x < Math.Min(Width, newWidth); x++)
                newMap[y * newWidth + x] = mapRegionIds[Index(x, y)];

        mapRegionIds = newMap;
        Width = newWidth;
        Height = newHeight;
    }
}