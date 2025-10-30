using DefaultNamespace;

namespace MapLib;

public readonly struct Tile
{
    public int X { get; init; }
    public int Y { get; init; }
    public TileType Type { get; init; }
    public byte TerritoryId { get; init; }

    public Tile(int x, int y, TileType type, byte territoryId)
    {
        X = x;
        Y = y;
        Type = type;
        TerritoryId = territoryId;
    }
}