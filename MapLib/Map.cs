using MapLib.Enum;
using MapLib.Interfaces;
using MapLib.Models;

namespace MapLib;

public class Map : IMap
{
    public const int Width = 1000;
    public const int Height = 1000;
    public const int MaxTerritories = 87;
    
    private TileType[,] _tileTypes;
    private int[,] _territoryIds;
    private readonly Dictionary<int, TerritoryInfo> _territories = new(MaxTerritories);
    public Map()
    {
        _tileTypes = new TileType[Width, Height];
        _territoryIds = new int[Width, Height];
    }
    
    /// <summary>
    /// Возвращает упакованное представление тайла в виде 64-битного целого числа.
    /// Младший байт содержит значение TileType, 
    /// следующие 32 бита — идентификатор территории territoryId.
    /// </summary>
    public long GetTile(int x, int y)
    {
        ValidateMapBounds(x, y);

        TileType type = _tileTypes[x, y];
        int territoryId = _territoryIds[x, y];
        
        return ((long)type) | ((long)territoryId << 8);
    }
    
    public TileType GetTileType(int x, int y)
    {
        ValidateMapBounds(x, y);

        return _tileTypes[x, y];
    }
    
    public int GetTerritoryId(int x, int y)
    {
        ValidateMapBounds(x, y);

        return _territoryIds[x, y];
    }

    public TerritoryInfo GetTerritoryInfo(int id)
    {
        if (_territories.TryGetValue(id, out var territory))
            return territory;
        
        throw new ArgumentException($"Unknown territory id: {id}");
    }
    
    public void SetTile(int x, int y, TileType type, int territoryId)
    {
        ValidateMapBounds(x, y);

        if (territoryId < 0 || territoryId >= MaxTerritories)
            throw new ArgumentOutOfRangeException($"Territory ID must be between 0 and {MaxTerritories - 1}");

        _tileTypes[x, y] = type;
        _territoryIds[x, y] = territoryId;
    }
    
    public TileType GetTileTypeFromPacked(long packedTile)
    {
        return (TileType)(packedTile & 0xFF);
    }

    public int GetTerritoryIdFromPacked(long packedTile)
    {
        return (int)(packedTile >> 8);
    }

    private void ValidateMapBounds(int width, int height)
    {
        if (width < 0 || width >= Width || height < 0 || height >= Height)
            throw new ArgumentOutOfRangeException($"Coordinates ({width}, {height}) are out of bounds");
    }
}