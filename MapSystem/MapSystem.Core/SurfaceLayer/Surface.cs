using MapSystem.Core.Common;

namespace MapSystem.Core.SurfaceLayer;

public sealed class Surface
{
    private readonly byte[] _tiles;

    private Surface(int width, int height, byte[] tiles)
    {
        Width = width;
        Height = height;
        _tiles = tiles;
    }

    public int Width { get; }

    public int Height { get; }

    public static Surface CreateFromCollection(int width, int height, IEnumerable<TileType> tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles, nameof(tiles));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width, nameof(width));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height, nameof(height));

        var tilesArray = tiles.ToArray();
        var expectedCount = width * height;
        if (tilesArray.Length != expectedCount)
            throw new ArgumentException(
                $"Количество тайлов ({tilesArray.Length}) не совпадает с размером карты " +
                $"{width} Х {height} ({expectedCount})",
                nameof(tiles));

        var tileBytes = new byte[expectedCount];
        for (var i = 0; i < expectedCount; i++) tileBytes[i] = (byte)tilesArray[i];

        return new Surface(width, height, tileBytes);
    }

    public TileType GetTileType(int x, int y)
    {
        ValidateHelpers.ValidatePoint(x, y, Width, Height);
        var index = ValidateHelpers.GetIndex(x, y, Width);
        return (TileType)_tiles[index];
    }

    public void SetTileType(int x, int y, TileType type)
    {
        ValidateHelpers.ValidatePoint(x, y, Width, Height);
        var index = ValidateHelpers.GetIndex(x, y, Width);
        _tiles[index] = (byte)type;
    }

    public void FillArea(int x, int y, int width, int height, TileType type)
    {
        ValidateHelpers.ValidateArea(x, y, width, height);
        ValidateHelpers.ValidateAreaBounds(x, y, width, height, Width, Height);

        var typeByte = (byte)type;

        for (var row = y; row < y + height; row++)
        {
            var startIndex = ValidateHelpers.GetIndex(x, row, Width);
            var rowSpan = new Span<byte>(_tiles, startIndex, width);
            rowSpan.Fill(typeByte);
        }
    }

    public bool CanPlaceInArea(int x, int y, int width, int height)
    {
        if (!ValidateHelpers.IsAreaInBounds(x, y, width, height, Width, Height))
            return false;

        for (var row = y; row < y + height; row++)
        for (var col = x; col < x + width; col++)
        {
            var tileType = GetTileType(col, row);
            if (!tileType.CanPlace())
                return false;
        }

        return true;
    }
}