using Xunit;

namespace MapSurface.Tests;

public class SurfaceLayerTests
{
    [Fact]
    public void Constructor_ValidDimensions_CreatesLayer()
    {
        var layer = new SurfaceLayer(1000, 1000);

        Assert.Equal(1000, layer.Width);
        Assert.Equal(1000, layer.Height);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(100, 0)]
    [InlineData(100, -1)]
    public void Constructor_InvalidDimensions_ThrowsArgumentException(int width, int height)
    {
        Assert.Throws<ArgumentException>(() => new SurfaceLayer(width, height));
    }

    [Fact]
    public void GetTileType_DefaultTile_ReturnsPlain()
    {
        var layer = new SurfaceLayer(10, 10);

        var tileType = layer.GetTileType(5, 5);

        Assert.Equal(TileType.Plain, tileType);
    }

    [Fact]
    public void SetTileType_ValidCoordinates_SetsTileType()
    {
        var layer = new SurfaceLayer(10, 10);

        layer.SetTileType(5, 5, TileType.Mountain);

        Assert.Equal(TileType.Mountain, layer.GetTileType(5, 5));
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(10, 5)]
    [InlineData(5, -1)]
    [InlineData(5, 10)]
    public void GetTileType_OutOfBounds_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        var layer = new SurfaceLayer(10, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetTileType(x, y));
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(10, 5)]
    [InlineData(5, -1)]
    [InlineData(5, 10)]
    public void SetTileType_OutOfBounds_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        var layer = new SurfaceLayer(10, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.SetTileType(x, y, TileType.Mountain));
    }

    [Fact]
    public void FillArea_ValidArea_FillsAllTiles()
    {
        var layer = new SurfaceLayer(10, 10);

        layer.FillArea(2, 2, 5, 5, TileType.Mountain);

        for (int y = 2; y < 7; y++)
        {
            for (int x = 2; x < 7; x++)
            {
                Assert.Equal(TileType.Mountain, layer.GetTileType(x, y));
            }
        }
    }

    [Fact]
    public void FillArea_ValidArea_DoesNotAffectOutsideTiles()
    {
        var layer = new SurfaceLayer(10, 10);

        layer.FillArea(2, 2, 5, 5, TileType.Mountain);

        Assert.Equal(TileType.Plain, layer.GetTileType(0, 0));
        Assert.Equal(TileType.Plain, layer.GetTileType(9, 9));
        Assert.Equal(TileType.Plain, layer.GetTileType(1, 5));
        Assert.Equal(TileType.Plain, layer.GetTileType(8, 5));
    }

    [Theory]
    [InlineData(-1, 0, 5, 5)]
    [InlineData(0, -1, 5, 5)]
    [InlineData(6, 0, 5, 5)]
    [InlineData(0, 6, 5, 5)]
    [InlineData(0, 0, 0, 5)]
    [InlineData(0, 0, 5, 0)]
    [InlineData(0, 0, -1, 5)]
    [InlineData(0, 0, 5, -1)]
    public void FillArea_InvalidArea_ThrowsException(int x, int y, int width, int height)
    {
        var layer = new SurfaceLayer(10, 10);

        Assert.ThrowsAny<Exception>(() => layer.FillArea(x, y, width, height, TileType.Mountain));
    }

    [Fact]
    public void CanPlaceObjectInArea_AllPlainTiles_ReturnsTrue()
    {
        var layer = new SurfaceLayer(10, 10);

        var canPlace = layer.CanPlaceObjectInArea(2, 2, 5, 5);

        Assert.True(canPlace);
    }

    [Fact]
    public void CanPlaceObjectInArea_AllMountainTiles_ReturnsFalse()
    {
        var layer = new SurfaceLayer(10, 10);
        layer.FillArea(0, 0, 10, 10, TileType.Mountain);

        var canPlace = layer.CanPlaceObjectInArea(2, 2, 5, 5);

        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlaceObjectInArea_OneMountainTile_ReturnsFalse()
    {
        var layer = new SurfaceLayer(10, 10);
        layer.SetTileType(4, 4, TileType.Mountain);

        var canPlace = layer.CanPlaceObjectInArea(2, 2, 5, 5);

        Assert.False(canPlace);
    }

    [Theory]
    [InlineData(-1, 0, 5, 5)]
    [InlineData(0, -1, 5, 5)]
    [InlineData(6, 0, 5, 5)]
    [InlineData(0, 6, 5, 5)]
    public void CanPlaceObjectInArea_InvalidArea_ThrowsException(int x, int y, int width, int height)
    {
        var layer = new SurfaceLayer(10, 10);

        Assert.ThrowsAny<Exception>(() => layer.CanPlaceObjectInArea(x, y, width, height));
    }

    [Fact]
    public void CreateFromTiles_ValidArray_CreatesLayer()
    {
        var tiles = new TileType[100];
        for (int i = 0; i < 100; i++)
        {
            tiles[i] = i % 2 == 0 ? TileType.Plain : TileType.Mountain;
        }

        var layer = SurfaceLayer.CreateFromTiles(10, 10, tiles);

        Assert.Equal(10, layer.Width);
        Assert.Equal(10, layer.Height);
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                int index = y * 10 + x;
                var expected = index % 2 == 0 ? TileType.Plain : TileType.Mountain;
                Assert.Equal(expected, layer.GetTileType(x, y));
            }
        }
    }

    [Fact]
    public void CreateFromTiles_ValidList_CreatesLayer()
    {
        var tiles = new List<TileType>();
        for (int i = 0; i < 100; i++)
        {
            tiles.Add(i % 2 == 0 ? TileType.Plain : TileType.Mountain);
        }

        var layer = SurfaceLayer.CreateFromTiles(10, 10, tiles);

        Assert.Equal(10, layer.Width);
        Assert.Equal(10, layer.Height);
    }

    [Fact]
    public void CreateFromTiles_NullArray_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SurfaceLayer.CreateFromTiles(10, 10, (TileType[])null));
    }

    [Fact]
    public void CreateFromTiles_WrongSize_ThrowsArgumentException()
    {
        var tiles = new TileType[50];

        Assert.Throws<ArgumentException>(() => SurfaceLayer.CreateFromTiles(10, 10, tiles));
    }

    [Fact]
    public void LargeMap_MemoryUsage_WithinLimits()
    {
        var layer = new SurfaceLayer(1000, 1000);
        
        layer.SetTileType(0, 0, TileType.Mountain);
        layer.SetTileType(999, 999, TileType.Mountain);
        
        Assert.Equal(TileType.Mountain, layer.GetTileType(0, 0));
        Assert.Equal(TileType.Mountain, layer.GetTileType(999, 999));
    }

    [Fact]
    public void FillArea_LargeArea_PerformsEfficiently()
    {
        var layer = new SurfaceLayer(1000, 1000);

        layer.FillArea(0, 0, 1000, 1000, TileType.Mountain);

        Assert.Equal(TileType.Mountain, layer.GetTileType(500, 500));
    }

    [Fact]
    public void CanPlaceObjectInArea_EdgeCases_WorksCorrectly()
    {
        var layer = new SurfaceLayer(10, 10);
        
        Assert.True(layer.CanPlaceObjectInArea(0, 0, 1, 1));
        Assert.True(layer.CanPlaceObjectInArea(9, 9, 1, 1));
        Assert.True(layer.CanPlaceObjectInArea(0, 0, 10, 10));
    }
}

