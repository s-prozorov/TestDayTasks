using MapLib.Enum;
using MapLib.Models;

namespace MapLib.Interfaces;

public interface IMap
{
    long GetTile(int x, int y);
    TileType GetTileType(int x, int y);
    int GetTerritoryId(int x, int y);
    TerritoryInfo GetTerritoryInfo(int id);
    void SetTile(int x, int y, TileType type, int territoryId);
    TileType GetTileTypeFromPacked(long packedTile);
    int GetTerritoryIdFromPacked(long packedTile);
}