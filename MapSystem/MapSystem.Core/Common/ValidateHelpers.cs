using MapSystem.Core.ObjectLayer;

namespace MapSystem.Core.Common;

internal static class ValidateHelpers
{
    // Общая валидация
    public static int GetIndex(int x, int y, int width)
    {
        return y * width + x;
    }

    public static void ValidatePoint(int x, int y, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x, nameof(x));
        ArgumentOutOfRangeException.ThrowIfNegative(y, nameof(y));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, width, nameof(x));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, height, nameof(y));
    }

    public static void ValidateArea(int x, int y, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x, nameof(x));
        ArgumentOutOfRangeException.ThrowIfNegative(y, nameof(y));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width, nameof(width));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height, nameof(height));
    }

    public static void ValidateAreaBounds(
        int x,
        int y,
        int width,
        int height,
        int mapWidth,
        int mapHeight)
    {
        var right = x + width;
        var bottom = y + height;

        if (right > mapWidth)
            throw new ArgumentOutOfRangeException(nameof(width),
                $"Область выходит за правую границу карты: "
                + $"Right={right}, MapWidth={mapWidth}");

        if (bottom > mapHeight)
            throw new ArgumentOutOfRangeException(
                nameof(height),
                $"Область выходит за нижнюю границу карты: " +
                $"Bottom={bottom}, MapHeight={mapHeight}");
    }

    public static bool IsAreaInBounds(
        int x,
        int y,
        int width,
        int height,
        int mapWidth,
        int mapHeight)
    {
        return x >= 0 &&
               y >= 0 &&
               width > 0 &&
               height > 0 &&
               x + width <= mapWidth &&
               y + height <= mapHeight;
    }

    // Специфичная для объектов
    public static bool Intersects(MapObject obj, int areaX, int areaY, int areaWidth, int areaHeight)
    {
        var objRight = obj.X + obj.Width - 1;
        var objBottom = obj.Y + obj.Height - 1;

        var areaRight = areaX + areaWidth - 1;
        var areaBottom = areaY + areaHeight - 1;

        var notIntersecting =
            objRight < areaX ||
            obj.X > areaRight ||
            objBottom < areaY ||
            obj.Y > areaBottom;

        return !notIntersecting;
    }

    public static bool ContainsPoint(MapObject obj, int x, int y)
    {
        return x >= obj.X &&
               x <= obj.X + obj.Width - 1 &&
               y >= obj.Y &&
               y <= obj.Y + obj.Height - 1;
    }

    public static (double centerX, double centerY) GetCenter(MapObject obj)
    {
        var centerX = obj.X + obj.Width / 2.0;
        var centerY = obj.Y + obj.Height / 2.0;
        return (centerX, centerY);
    }

    // Специфичная для регионов
    public static int CalculateRegionsPerSide(int targetRegionCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetRegionCount, nameof(targetRegionCount));

        var regionsPerSide = (int)Math.Ceiling(Math.Sqrt(targetRegionCount));
        return regionsPerSide;
    }


    public static void ValidateRegionDivision(int mapWidth, int mapHeight, int regionsPerSide)
    {
        if (mapWidth % regionsPerSide != 0)
            throw new ArgumentException(
                $"Ширина карты ({mapWidth}) не делится нацело на количество регионов по стороне ({regionsPerSide}). " +
                $"Остаток: {mapWidth % regionsPerSide}",
                nameof(regionsPerSide));

        if (mapHeight % regionsPerSide != 0)
            throw new ArgumentException(
                $"Высота карты ({mapHeight}) не делится нацело на количество регионов по стороне ({regionsPerSide}). " +
                $"Остаток: {mapHeight % regionsPerSide}",
                nameof(regionsPerSide));
    }
}