using TASK_1_1.Layers;

namespace TASK_1_1.Tests;
public class MapTests
{
    [Fact]
    public void AddSurfaceLayer_WithWrongSize_ShouldThrow()
    {
        var map = new Map(5, 5);
        var layer = new Surface(4, 4);

        var ex = Assert.Throws<ArgumentException>(() => map.AddSurfaceLayer("Surface1", layer));
        Assert.Equal("Размер поверхности должен совпадать с размером карты.", ex.Message);
    }

    [Fact]
    public void AddAndGetSurfaceLayer_ShouldWork()
    {
        var map = new Map(5, 5);
        var layer = new Surface(5, 5);

        map.AddSurfaceLayer("Surface1", layer);
        var retrieved = map.GetSurfaceLayer("Surface1");

        Assert.Equal(layer, retrieved);
    }

    [Fact]
    public void GetSurfaceLayer_NonExisting_ShouldThrow()
    {
        var map = new Map(5, 5);

        var ex = Assert.Throws<KeyNotFoundException>(() => map.GetSurfaceLayer("NotExist"));
        Assert.Contains("Поверхность 'NotExist' не наидена.", ex.Message);
    }

    [Fact]
    public void CanPlaceObject_ShouldDelegateToLayer()
    {
        var map = new Map(5, 5);
        var layer = new Surface(5, 5);
        map.AddSurfaceLayer("Surface1", layer);

        layer.SetTileType(2, 2, TileEnum.Mountain);

        bool canPlace = map.CanPlaceObject("Surface1", 0, 0, 4, 4);
        Assert.False(canPlace);

        canPlace = map.CanPlaceObject("Surface1", 0, 0, 1, 1);
        Assert.True(canPlace);
    }

    [Fact]
    public void Resize_ShouldResizeMapAndLayer()
    {
        var map = new Map(2, 2);
        var layer = new Surface(2, 2);
        layer.SetTileType(0, 0, TileEnum.Mountain);
        map.AddSurfaceLayer("Surface1", layer);

        map.Resize(3, 3, TileEnum.Plain);

        Assert.Equal(3, map.Width);
        Assert.Equal(3, map.Height);
        var resizedLayer = map.GetSurfaceLayer("Surface1");
        Assert.Equal(TileEnum.Mountain, resizedLayer.GetTileType(0, 0));
        Assert.Equal(TileEnum.Plain, resizedLayer.GetTileType(2, 2));
    }
}
