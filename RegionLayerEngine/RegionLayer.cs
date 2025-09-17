namespace RegionLayerEngine;

public class RegionLayer
{
    private readonly ushort[,] regionMap;
    private readonly IDictionary<ushort, Region> regions = new Dictionary<ushort, Region>();
    
    public int Width { get; }
    public int Height { get; }

    public RegionLayer(int width, int height, int regionSize)
    {
        Width = width;
        Height = height;
        regionMap = new ushort[width, height];
        GenerateRegions(regionSize);
    }

    private void GenerateRegions(int regionSize)
    {
        ushort counter = 0;
        for (int x = 0; x < Width; x += regionSize)
        for (int y = 0; y < Height; y += regionSize)
        {
            var region = new Region(counter, $"Region {counter}");
            regions[counter] = region;
                
            for (int i = x; i < x + regionSize && i < Width; i++)
            for (int j = y; j < y + regionSize && j < Height; j++)
                regionMap[i, j] = counter;
                
            counter++;
        }
    }

    public ushort GetRegionId(int x, int y) => 
        (x >= 0 && x < Width && y >= 0 && y < Height) ? regionMap[x, y] : (ushort)0;

    public Region GetRegionMetadata(ushort id) => regions.TryGetValue(id, out var region) ? region : null;

    public IEnumerable<ushort> GetRegionsInArea(int x1, int y1, int x2, int y2)
    {
        var found = new HashSet<ushort>();
        for (int x = x1; x <= x2; x++)
        for (int y = y1; y <= y2; y++)
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                found.Add(regionMap[x, y]);
        return found;
    }
}