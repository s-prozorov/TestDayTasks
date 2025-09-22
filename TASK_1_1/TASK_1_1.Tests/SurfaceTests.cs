using TASK_1_1.Layers;

namespace TASK_1_1.Tests;

public class SurfaceTests
{
    [Fact]
    public void GetTile_ShouldReturnCorrectTile()
    {
        var surface = new Surface(5, 5);
        surface.SetTileType(2, 3, TileEnum.Mountain);

        var tile = surface.GetTileType(2, 3);

        Assert.Equal(TileEnum.Mountain, tile);
    }

    [Fact]
    public void SetTile_ShouldChangeTileValue()
    {
        var surface = new Surface(5, 5);

        surface.SetTileType(1, 1, TileEnum.Mountain);
        Assert.Equal(TileEnum.Mountain, surface.GetTileType(1, 1));

        surface.SetTileType(1, 1, TileEnum.Plain);
        Assert.Equal(TileEnum.Plain, surface.GetTileType(1, 1));
    }

    [Fact]
    public void FromArray_ShouldCreateSurfaceFromExistingArray()
    {
        var array = new TileEnum[2, 2]
        {
            { TileEnum.Plain, TileEnum.Mountain },
            { TileEnum.Mountain, TileEnum.Plain }
        };

        var surface = new Surface(100,100);
        surface.FromArray(array);

        Assert.Equal(TileEnum.Plain, surface.GetTileType(0, 0));
        Assert.Equal(TileEnum.Mountain, surface.GetTileType(0, 1));
        Assert.Equal(TileEnum.Mountain, surface.GetTileType(1, 0));
        Assert.Equal(TileEnum.Plain, surface.GetTileType(1, 1));
    }

    [Fact]
    public void FillArea_ShouldFillCorrectRegion()
    {
        var surface = new Surface(5, 5);
        surface.FillArea(1, 1, 3, 3, TileEnum.Mountain);

        // Проверяем область
        for (int x = 1; x <= 3; x++)
            for (int y = 1; y <= 3; y++)
                Assert.Equal(TileEnum.Mountain, surface.GetTileType(x, y));

        // Вне области остаётся Plain
        Assert.Equal(TileEnum.Plain, surface.GetTileType(0, 0));
        Assert.Equal(TileEnum.Plain, surface.GetTileType(4, 4));
    }

    [Fact]
    public void CanPlaceObject_ShouldReturnTrueForPlain()
    {
        var surface = new Surface(3, 3);

        // Все тайлы Plain
        bool canPlace = surface.CanPlaceObject(0, 0, 2, 2);

        Assert.True(canPlace);
    }

    [Fact]
    public void CanPlaceObject_ShouldReturnFalseIfAnyMountain()
    {
        var surface = new Surface(3, 3);
        surface.SetTileType(1, 1, TileEnum.Mountain);

        bool canPlace = surface.CanPlaceObject(0, 0, 2, 2);

        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlaceObject_ShouldHandlePartialArea()
    {
        var surface = new Surface(3, 3);
        surface.SetTileType(0, 0, TileEnum.Mountain);

        bool canPlace = surface.CanPlaceObject(1, 1, 2, 2);

        Assert.True(canPlace);
    }
}
