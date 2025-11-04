using MapLib.Core.Models;
using MapLib.Core.Models.TerritoryModel;
using MapLib.Core.Models.TileModel;

namespace MapLib.Core.Interfaces;

public interface IMapService
{
    Tile GetTile(int x, int y);
    
    TileType GetTileType(int x, int y);
    
    int GetTerritoryId(int x, int y);

    Territory? GetTerritoryInfo(int id);
    
    List<Territory> GetTerritoriesInArea(int x1, int x2, int y1, int y2);
}