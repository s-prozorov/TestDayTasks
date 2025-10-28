using System.Runtime.CompilerServices;

namespace Surface;



internal static class SurfaceHelpers
{
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
        int right = x + width;
        int bottom = y + height;
        
        if (right > mapWidth)
        {
            throw new ArgumentOutOfRangeException(nameof(width), 
                $"Область выходит за правую границу карты: " 
                + $"Right={right}, MapWidth={mapWidth}");
        }
        
        if (bottom > mapHeight)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                $"Область выходит за нижнюю границу карты: " +
                $"Bottom={bottom}, MapHeight={mapHeight}");
        }
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
}