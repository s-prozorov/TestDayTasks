namespace MapObjects;

public sealed class MapObject
{
    public string Id { get; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public MapObject(string id, int x, int y, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Object ID cannot be null or empty", nameof(id));
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));

        Id = id;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public void UpdatePosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void UpdateSize(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));

        Width = width;
        Height = height;
    }

    public bool ContainsPoint(int x, int y)
    {
        return x >= X && x < X + Width && y >= Y && y < Y + Height;
    }

    public bool IntersectsArea(int areaX, int areaY, int areaWidth, int areaHeight)
    {
        return X < areaX + areaWidth &&
               X + Width > areaX &&
               Y < areaY + areaHeight &&
               Y + Height > areaY;
    }

    public bool IsFullyInArea(int areaX, int areaY, int areaWidth, int areaHeight)
    {
        return X >= areaX &&
               Y >= areaY &&
               X + Width <= areaX + areaWidth &&
               Y + Height <= areaY + areaHeight;
    }
}

