using MapLib.Core.Models.TerritoryModel;
using MapLib.Core.Models.TileModel;
using MapLib.Core.Services;

namespace MapLib.Core.Models.MapModel;

public class Map
{
    private const int WIDTH = 1000;
    private const int HEIGHT = 1000;
    private const int TERRITORY_COUNT = 87;
    
    private readonly Tile[,] tiles;
    private readonly Dictionary<int, Territory> territories;
    
    public Tile Tile(int x, int y) => tiles[x, y];
    
    public Territory? GetTerritoryInfo(int id)
    {
        return territories.GetValueOrDefault(id);
    }

    public List<Territory> GetTerritoriesInArea(int x1, int x2, int y1, int y2)
    {
        int minX = Math.Max(0, Math.Min(x1, x2));
        int maxX = Math.Min(999, Math.Max(x1, x2));
        int minY = Math.Max(0, Math.Min(y1, y2));
        int maxY = Math.Min(999, Math.Max(y1, y2));
        
        var territoryIds = new HashSet<int>();
        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                var tile = tiles[x, y];
                territoryIds.Add(tile.TerritoryId);
            }
        }
        
        var territoriesInArea = new List<Territory>();
        foreach (var id in territoryIds)
        {
            var territory = GetTerritoryInfo(id);
            if (territory != null)
            {
                territoriesInArea.Add(territory);
            }
        }
        
        
        return territoriesInArea;
    }

    public Map()
    {
        tiles = new Tile[WIDTH, HEIGHT];
        territories = new Dictionary<int, Territory>();
        TerritoryGenService.GenerateTerritories(TERRITORY_COUNT, territories, tiles);
    }
}