using MapSystem.Core.SurfaceLayer;

namespace MapSystem.Core.Tests;

public class SurfaceTests
{
    [Fact]
    public void CreateFromCollection_ValidSize_ReturnsLayer()
    {
        var tiles = new TileType[100 * 100];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(100, 100, tiles);

        Assert.Equal(100, layer.Width);
        Assert.Equal(100, layer.Height);
    }

    [Fact]
    public void CreateFromCollection_WithFlatFill_FillsWithFlat()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        Assert.Equal(TileType.Flat, layer.GetTileType(0, 0));
        Assert.Equal(TileType.Flat, layer.GetTileType(9, 9));
        Assert.Equal(TileType.Flat, layer.GetTileType(5, 5));
    }

    [Fact]
    public void CreateFromCollection_WithMountainFill_FillsWithMountain()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Mountain);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        Assert.Equal(TileType.Mountain, layer.GetTileType(0, 0));
        Assert.Equal(TileType.Mountain, layer.GetTileType(5, 5));
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(100, 0)]
    [InlineData(100, -1)]
    public void CreateFromCollection_InvalidSize_ThrowsException(int width, int height)
    {
        var tiles = new TileType[Math.Max(1, width * height)];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Surface.CreateFromCollection(width, height, tiles));
    }

    [Fact]
    public void CreateFromCollection_ValidData_ReturnsLayer()
    {
        var tiles = new[]
        {
            TileType.Flat, TileType.Mountain, TileType.Flat,
            TileType.Mountain, TileType.Flat, TileType.Mountain,
            TileType.Flat, TileType.Mountain, TileType.Flat
        };

        var layer = Surface.CreateFromCollection(3, 3, tiles);

        Assert.Equal(3, layer.Width);
        Assert.Equal(3, layer.Height);
        Assert.Equal(TileType.Flat, layer.GetTileType(0, 0));
        Assert.Equal(TileType.Mountain, layer.GetTileType(1, 0));
        Assert.Equal(TileType.Flat, layer.GetTileType(2, 2));
    }

    [Fact]
    public void CreateFromCollection_NullCollection_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Surface.CreateFromCollection(10, 10, null!));
    }

    [Fact]
    public void CreateFromCollection_WrongSize_ThrowsException()
    {
        var tiles = new[] { TileType.Flat, TileType.Mountain };

        Assert.Throws<ArgumentException>(() =>
            Surface.CreateFromCollection(10, 10, tiles));
    }

    [Fact]
    public void GetTileType_ValidCoordinates_ReturnsType()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);
        layer.SetTileType(5, 5, TileType.Mountain);

        var type = layer.GetTileType(5, 5);

        Assert.Equal(TileType.Mountain, type);
    }

    [Fact]
    public void SetTileType_ValidCoordinates_SetsType()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        layer.SetTileType(3, 7, TileType.Mountain);

        Assert.Equal(TileType.Mountain, layer.GetTileType(3, 7));
        Assert.Equal(TileType.Flat, layer.GetTileType(3, 6));
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    [InlineData(10, 5)]
    [InlineData(5, 10)]
    public void GetTileType_OutOfBounds_ThrowsException(int x, int y)
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetTileType(x, y));
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    [InlineData(10, 5)]
    [InlineData(5, 10)]
    public void SetTileType_OutOfBounds_ThrowsException(int x, int y)
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            layer.SetTileType(x, y, TileType.Mountain));
    }

    [Fact]
    public void FillArea_ValidArea_FillsCorrectly()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        layer.FillArea(2, 2, 3, 3, TileType.Mountain);

        Assert.Equal(TileType.Mountain, layer.GetTileType(2, 2));
        Assert.Equal(TileType.Mountain, layer.GetTileType(3, 3));
        Assert.Equal(TileType.Mountain, layer.GetTileType(4, 4));
        Assert.Equal(TileType.Flat, layer.GetTileType(1, 1));
        Assert.Equal(TileType.Flat, layer.GetTileType(5, 5));
    }

    [Fact]
    public void FillArea_EntireMap_FillsAllTiles()
    {
        var tiles = new TileType[5 * 5];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(5, 5, tiles);

        layer.FillArea(0, 0, 5, 5, TileType.Mountain);

        for (var y = 0; y < 5; y++)
        for (var x = 0; x < 5; x++)
            Assert.Equal(TileType.Mountain, layer.GetTileType(x, y));
    }

    [Fact]
    public void FillArea_SingleTile_FillsOnlyOne()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        layer.FillArea(5, 5, 1, 1, TileType.Mountain);

        Assert.Equal(TileType.Mountain, layer.GetTileType(5, 5));
        Assert.Equal(TileType.Flat, layer.GetTileType(4, 5));
        Assert.Equal(TileType.Flat, layer.GetTileType(6, 5));
    }

    [Theory]
    [InlineData(0, 0, 0, 5)]
    [InlineData(0, 0, 5, 0)]
    [InlineData(0, 0, -1, 5)]
    [InlineData(0, 0, 5, -1)]
    public void FillArea_InvalidSize_ThrowsException(int x, int y, int width, int height)
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            layer.FillArea(x, y, width, height, TileType.Mountain));
    }

    [Fact]
    public void FillArea_OutOfBounds_ThrowsException()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            layer.FillArea(8, 8, 5, 5, TileType.Mountain));
    }

    [Fact]
    public void CanPlaceInArea_AllFlat_ReturnsTrue()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        var canPlace = layer.CanPlaceInArea(2, 2, 3, 3);

        Assert.True(canPlace);
    }

    [Fact]
    public void CanPlaceInArea_AllMountain_ReturnsFalse()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Mountain);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        var canPlace = layer.CanPlaceInArea(2, 2, 3, 3);

        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlaceInArea_OneMountainInside_ReturnsFalse()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);
        layer.SetTileType(5, 5, TileType.Mountain);

        var canPlace = layer.CanPlaceInArea(4, 4, 3, 3);

        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlaceInArea_MountainOutside_ReturnsTrue()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);
        layer.SetTileType(0, 0, TileType.Mountain);

        var canPlace = layer.CanPlaceInArea(5, 5, 3, 3);

        Assert.True(canPlace);
    }

    [Fact]
    public void CanPlaceInArea_OutOfBounds_ReturnsFalse()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        var canPlace = layer.CanPlaceInArea(8, 8, 5, 5);

        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlaceInArea_NegativeCoordinates_ReturnsFalse()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        var canPlace = layer.CanPlaceInArea(-1, 5, 3, 3);

        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlaceInArea_NegativeWidth_ReturnsFalse()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        var canPlace = layer.CanPlaceInArea(5, 5, -1, 3);

        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlaceInArea_ZeroWidth_ReturnsFalse()
    {
        var tiles = new TileType[10 * 10];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(10, 10, tiles);

        var canPlace = layer.CanPlaceInArea(5, 5, 0, 3);

        Assert.False(canPlace);
    }

    [Fact]
    public void CreateFromCollection_1000x1000_Success()
    {
        var tiles = new TileType[1000 * 1000];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(1000, 1000, tiles);

        Assert.Equal(1000, layer.Width);
        Assert.Equal(1000, layer.Height);
        Assert.Equal(TileType.Flat, layer.GetTileType(0, 0));
        Assert.Equal(TileType.Flat, layer.GetTileType(999, 999));
    }

    [Fact]
    public void FillArea_LargeArea_Success()
    {
        var tiles = new TileType[1000 * 1000];
        Array.Fill(tiles, TileType.Flat);
        var layer = Surface.CreateFromCollection(1000, 1000, tiles);

        layer.FillArea(0, 0, 1000, 1000, TileType.Mountain);

        Assert.Equal(TileType.Mountain, layer.GetTileType(500, 500));
        Assert.Equal(TileType.Mountain, layer.GetTileType(0, 0));
        Assert.Equal(TileType.Mountain, layer.GetTileType(999, 999));
    }
}