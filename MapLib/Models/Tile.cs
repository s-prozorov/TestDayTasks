using MapLib.Enum;

namespace MapLib.Models;

public readonly struct Tile(int x, int y, TileType type, byte territoryId)
{
    public int X { get; init; } = x;
    public int Y { get; init; } = y;
    public TileType Type { get; init; } = type;
    public byte TerritoryId { get; init; } = territoryId;

    /// <summary>
    /// Упаковывает данные тайла в long для сетевой передачи.
    /// Формат: [младший байт: TileType][следующие 32 бита: TerritoryId]
    /// </summary>
    public long Pack()
    {
        return ((long)Type) | ((long)TerritoryId << 8);
    }

    /// <summary>
    /// Создаёт тайл из упакованного long значения.
    /// </summary>
    public static Tile Unpack(long packedTile, int x, int y)
    {
        var type = (TileType)(packedTile & 0xFF);
        var territoryId = (byte)(packedTile >> 8);
        return new Tile(x, y, type, territoryId);
    }
}