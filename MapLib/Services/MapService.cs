using MapLib.Core.Interfaces;
using MapLib.Core.Models;
using MapLib.Core.Models.MapModel;
using MapLib.Core.Models.TerritoryModel;
using MapLib.Core.Models.TileModel;

namespace MapLib.Services;

public class MapService(MapProvider mapProvider) : IMapService
{
    private Map map = mapProvider.Instance;
    
    public Tile GetTile(int x, int y)
    {
        ValidateCoordinates(x, y);
        return map.Tile(x, y);
    }

    public TileType GetTileType(int x, int y)
    {
        ValidateCoordinates(x, y);
        return map.Tile(x, y).Type;
    }

    public int GetTerritoryId(int x, int y)
    { 
        ValidateCoordinates(x, y);
        return map.Tile(x, y).TerritoryId;
    }

    public Territory? GetTerritoryInfo(int id)
    {
        return map.GetTerritoryInfo(id);
    }

    public List<Territory> GetTerritoriesInArea(int x1, int x2, int y1, int y2)
    {
        ValidateCoordinates(x1, y1);
        ValidateCoordinates(x2, y2);

        return map.GetTerritoriesInArea(x1, x2, y1, y2);
    }

    private void ValidateCoordinates(int x, int y)
    {
        if (x < 0 || x >= 1000 || y < 0 || y >= 1000)
            throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are out of map bounds");
    }
}