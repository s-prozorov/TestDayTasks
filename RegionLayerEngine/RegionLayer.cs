namespace RegionLayerEngine;

public class RegionLayer
{
    private readonly ushort[,] _tileToRegionMap;
    private readonly Dictionary<ushort, Region> _regions = new();
    private readonly int _regionSize;
    
    public RegionLayer(int width, int height, int regionSize)
    {
        _tileToRegionMap = new ushort[width, height];
        _regionSize = regionSize;
    }
    
    public void GenerateRegions(string[] regionNames)
    {
        // Генерация регионов с равномерным распределением
        var regionId = 0;
        for (int x = 0; x < _tileToRegionMap.GetLength(0); x += _regionSize)
        {
            for (int y = 0; y < _tileToRegionMap.GetLength(1); y += _regionSize)
            {
                var region = new Region((ushort)regionId, regionNames[regionId]);
                _regions.Add(region.Id, region);
                
                // Заполняем тайлы региона
                for (int rx = 0; rx < _regionSize; rx++)
                {
                    for (int ry = 0; ry < _regionSize; ry++)
                    {
                        if (x + rx < _tileToRegionMap.GetLength(0) &&
                            y + ry < _tileToRegionMap.GetLength(1))
                        {
                            _tileToRegionMap[x + rx, y + ry] = region.Id;
                            region.Tiles.Add((x + rx, y + ry));
                        }
                    }
                }
                regionId++;
            }
        }
    }
    
    public ushort GetRegionId(int x, int y)
    {
        return _tileToRegionMap[x, y];
    }

    public Region GetRegion(ushort id)
    {
        return _regions.TryGetValue(id, out var region) ? region : null;
    }

    public bool IsTileInRegion(int x, int y, ushort regionId)
    {
        return _tileToRegionMap[x, y] == regionId;
    }

    public List<Region> GetRegionsInArea(int x1, int y1, int x2, int y2)
    {
        var result = new HashSet<ushort>();
    
        for (int x = x1; x <= x2; x++)
        {
            for (int y = y1; y <= y2; y++)
            {
                result.Add(_tileToRegionMap[x, y]);
            }
        }
    
        return result.Select(id => _regions[id]).ToList();
    }
}