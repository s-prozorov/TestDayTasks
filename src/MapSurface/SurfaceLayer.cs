namespace MapSurface;

public sealed class SurfaceLayer : ISurfaceLayer
{
    private readonly byte[] _tiles;
    private readonly int _width;
    private readonly int _height;

    public int Width => _width;
    public int Height => _height;

    public SurfaceLayer(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));

        _width = width;
        _height = height;
        _tiles = new byte[width * height];
    }

    public static SurfaceLayer CreateFromTiles(int width, int height, IReadOnlyList<TileType> tiles)
    {
        if (tiles == null)
            throw new ArgumentNullException(nameof(tiles));
        if (tiles.Count != width * height)
            throw new ArgumentException($"Tiles count must be {width * height}", nameof(tiles));

        var layer = new SurfaceLayer(width, height);
        for (int i = 0; i < tiles.Count; i++)
        {
            layer._tiles[i] = (byte)tiles[i];
        }
        return layer;
    }

    public static SurfaceLayer CreateFromTiles(int width, int height, TileType[] tiles)
    {
        if (tiles == null)
            throw new ArgumentNullException(nameof(tiles));
        if (tiles.Length != width * height)
            throw new ArgumentException($"Tiles array length must be {width * height}", nameof(tiles));

        var layer = new SurfaceLayer(width, height);
        for (int i = 0; i < tiles.Length; i++)
        {
            layer._tiles[i] = (byte)tiles[i];
        }
        return layer;
    }

    public TileType GetTileType(int x, int y)
    {
        ValidateCoordinates(x, y);
        return (TileType)_tiles[y * _width + x];
    }

    public void SetTileType(int x, int y, TileType tileType)
    {
        ValidateCoordinates(x, y);
        _tiles[y * _width + x] = (byte)tileType;
    }

    public void FillArea(int x, int y, int width, int height, TileType tileType)
    {
        ValidateArea(x, y, width, height);

        byte tileValue = (byte)tileType;
        for (int dy = 0; dy < height; dy++)
        {
            int rowStart = (y + dy) * _width + x;
            Array.Fill(_tiles, tileValue, rowStart, width);
        }
    }

    public bool CanPlaceObjectInArea(int x, int y, int width, int height)
    {
        ValidateArea(x, y, width, height);

        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                var tileType = (TileType)_tiles[(y + dy) * _width + (x + dx)];
                var properties = TileProperties.GetProperties(tileType);
                if (!properties.CanPlaceObject)
                    return false;
            }
        }
        return true;
    }

    private void ValidateCoordinates(int x, int y)
    {
        if (x < 0 || x >= _width)
            throw new ArgumentOutOfRangeException(nameof(x), $"X coordinate must be between 0 and {_width - 1}");
        if (y < 0 || y >= _height)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y coordinate must be between 0 and {_height - 1}");
    }

    private void ValidateArea(int x, int y, int width, int height)
    {
        if (x < 0 || x >= _width)
            throw new ArgumentOutOfRangeException(nameof(x), $"X coordinate must be between 0 and {_width - 1}");
        if (y < 0 || y >= _height)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y coordinate must be between 0 and {_height - 1}");
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));
        if (x + width > _width)
            throw new ArgumentOutOfRangeException(nameof(width), $"Area exceeds map bounds: x + width = {x + width}, map width = {_width}");
        if (y + height > _height)
            throw new ArgumentOutOfRangeException(nameof(height), $"Area exceeds map bounds: y + height = {y + height}, map height = {_height}");
    }
}

