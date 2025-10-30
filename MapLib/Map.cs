using MapLib.Enum;
using MapLib.Interfaces;
using MapLib.Models;

namespace MapLib;

public class Map : IMap
{
    public const int Width = 1000;
    public const int Height = 1000;
    public const int MaxTerritories = 87;
    
    private readonly Tile[,] _tiles;
    private readonly Dictionary<int, TerritoryInfo> _territories;

    public Map()
    {
        _tiles = new Tile[Width, Height];
        _territories = new Dictionary<int, TerritoryInfo>(MaxTerritories);
    }

    /// <summary>
    /// Возвращает упакованное представление тайла в виде 64-битного целого числа.
    /// Младший байт содержит значение TileType, 
    /// следующие 32 бита — идентификатор территории territoryId.
    /// </summary>
    public long GetTile(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _tiles[x, y].Pack();
    }
    
    public TileType GetTileType(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _tiles[x, y].Type;
    }
    
    public int GetTerritoryId(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _tiles[x, y].TerritoryId;
    }
    
    public TerritoryInfo GetTerritoryInfo(int id)
    {
        if (_territories.TryGetValue(id, out var territory))
            return territory;
        
        throw new ArgumentException($"Unknown territory id: {id}", nameof(id));
    }
    
    public void SetTile(int x, int y, TileType type, int territoryId)
    {
        ValidateCoordinates(x, y);
        
        if (territoryId < 0 || territoryId >= MaxTerritories)
        {
            throw new ArgumentOutOfRangeException(
                nameof(territoryId),
                $"Territory ID must be between 0 and {MaxTerritories - 1}");
        }

        _tiles[x, y] = new Tile(x, y, type, (byte)territoryId);
    }
    
    public TileType GetTileTypeFromPacked(long packedTile)
    {
        return (TileType)(packedTile & 0xFF);
    }
    
    public int GetTerritoryIdFromPacked(long packedTile)
    {
        return (int)(packedTile >> 8);
    }

    /// <summary>
    /// Инициализирует информацию о территориях.
    /// Должен быть вызван после генерации карты.
    /// </summary>
    public void InitializeTerritories(IEnumerable<TerritoryInfo> territories)
    {
        _territories.Clear();
        
        foreach (var territory in territories)
        {
            if (territory.Id < 0 || territory.Id >= MaxTerritories)
            {
                throw new ArgumentException(
                    $"Territory ID {territory.Id} is out of valid range [0, {MaxTerritories})",
                    nameof(territories));
            }
            
            _territories[territory.Id] = territory;
        }
    }

    private void ValidateCoordinates(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            throw new ArgumentOutOfRangeException(
                $"Coordinates ({x}, {y}) are out of bounds. Valid range: [0, {Width}) x [0, {Height})");
        }
    }
}