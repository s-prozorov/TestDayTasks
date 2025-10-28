using System.Runtime.CompilerServices;

namespace Surface;


public static class TileTypeExtensions
{
    public static bool CanPlace(this TileType type)
    {
        return type switch
        {
            TileType.Flat => true,
            TileType.Mountain => false,
            _ => throw new ArgumentException($"Неизвестный тип тайла: {type}", nameof(type))
        };
    }
}