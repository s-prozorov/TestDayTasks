namespace Surface;

public sealed class Surface
{
    private readonly byte[] _tiles;
    private readonly int _width;
    private readonly int _height;
    
    public int Width => _width;
    public int Height => _height;
    
    private Surface(int width, int height, byte[] tiles)
    {
        _width = width;
        _height = height;
        _tiles = tiles;
    }

    public static Surface CreateFromCollection(int width, int height, IEnumerable<TileType> tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles, nameof(tiles));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width, nameof(width));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height, nameof(height));
        
        TileType[] tilesArray = tiles.ToArray();
        int expectedCount = width * height;
        if (tilesArray.Length != expectedCount)
        {
            throw new ArgumentException(
                $"Количество тайлов ({tilesArray.Length}) не совпадает с размером карты " +
                $"{width} Х {height} ({expectedCount})",
                nameof(tiles));
        }
        
        byte[] tileBytes = new byte[expectedCount];
        for (int i = 0; i < expectedCount; i++)
        {
            tileBytes[i] = (byte)tilesArray[i];
        }

        return new Surface(width, height, tileBytes);
    }
    
    public TileType GetTileType(int x, int y)
    {
        SurfaceHelpers.ValidatePoint(x, y, _width, _height);
        int index = SurfaceHelpers.GetIndex(x, y, _width);
        return (TileType)_tiles[index];
    }
    
    public void SetTileType(int x, int y, TileType type)
    {
        SurfaceHelpers.ValidatePoint(x, y, _width, _height);
        int index = SurfaceHelpers.GetIndex(x, y, _width);
        _tiles[index] = (byte)type;
    }
    
    public void FillArea(int x, int y, int width, int height, TileType type)
    {
        SurfaceHelpers.ValidateAreaBounds(x, y, width, height, _width, _height);

        byte typeByte = (byte)type;
        
        for (int row = y; row < y + height; row++)
        {
            int startIndex = SurfaceHelpers.GetIndex(x, row, _width);
            Span<byte> rowSpan = new Span<byte>(_tiles, startIndex, width);
            rowSpan.Fill(typeByte);
        }
    }
    
    public bool CanPlaceInArea(int x, int y, int width, int height)
    {
        if (!SurfaceHelpers.IsAreaInBounds(x, y, width, height, _width, _height))
            return false;
        
        for (int row = y; row < y + height; row++)
        {
            for (int col = x; col < x + width; col++)
            {
                TileType tileType = GetTileType(col, row);
                if (!tileType.CanPlace())
                    return false;
            }
        }

        return true;
    }
}