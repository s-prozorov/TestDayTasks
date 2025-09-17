using System.Diagnostics;
using System.Drawing;
using TileMapEngine.Enums;
using TileMapEngine.Models;

namespace TileMapProjectTests;

public class Tests
{
    private const int Width = 1000;
    private const int Height = 1000;
    private TileLayer _layer;
    
    [SetUp]
    public void Setup()
    {
        _layer = new TileLayer(Width, Height);
    }

    [Test]
    public void GetTileType_DefaultIsPlain()
    {
        Assert.AreEqual(SurfaceType.Plain, _layer.GetTile(0, 0));
        Assert.AreEqual(SurfaceType.Plain, _layer.GetTile(999, 999));
    }
    
    [Test]
    public void GetTileType_OutOfBounds_Throws()
    {
        Assert.Catch(typeof(ArgumentOutOfRangeException), () => _layer.GetTile(-1, 999));
    }
    
    [Test]
    public void SetAndGetTileType_CorrectlyUpdatesValue()
    {
        _layer.SetTile(10, 20, SurfaceType.Mountain);
        Assert.AreEqual(SurfaceType.Mountain, _layer.GetTile(10, 20));

        _layer.SetTile(10, 20, SurfaceType.Plain);
        Assert.AreEqual(SurfaceType.Plain, _layer.GetTile(10, 20));
    }
    
    [Test]
    public void FromEnumerable_CreatesCorrectLayer()
    {
        var tiles = new List<(int, int, SurfaceType)>
        {
            (1, 1, SurfaceType.Mountain),
            (2, 2, SurfaceType.Plain)
        };

        var layer = TileLayer.FromEnumerable(tiles, 3, 3);

        Assert.AreEqual(SurfaceType.Mountain, layer.GetTile(1, 1));
        Assert.AreEqual(SurfaceType.Plain, layer.GetTile(2, 2));
        Assert.AreEqual(3, layer.Width);
        Assert.AreEqual(3, layer.Height);
    }
    
    [Test]
    public void FillRegion_UpdatesCorrectArea()
    {
        var rect = new Rectangle(10, 10, 20, 20);
        _layer.FillRegion(rect, SurfaceType.Mountain);

        for (int y = 10; y < 30; y++)
        {
            for (int x = 10; x < 30; x++)
            {
                Assert.AreEqual(SurfaceType.Mountain, _layer.GetTile(x, y));
            }
        }

        Assert.AreEqual(SurfaceType.Plain, _layer.GetTile(9, 10));
        Assert.AreEqual(SurfaceType.Plain, _layer.GetTile(30, 30));
    }
    
    [Test]
    public void CanPlaceObjectInArea_ReturnsCorrectResults()
    {
        var area1 = new Rectangle(0, 0, 10, 10);
        Assert.IsTrue(_layer.CanPlaceObjectInArea(area1));

        _layer.SetTile(5, 5, SurfaceType.Mountain);
        Assert.IsFalse(_layer.CanPlaceObjectInArea(area1));

        var singleTile = new Rectangle(5, 5, 6, 6);
        Assert.IsFalse(_layer.CanPlaceObjectInArea(singleTile));
    }
    
    [Test]
    public void Performance_AccessTime([Values(0, 999, 500)] int x, [Values(0,999, 500)] int y)
    {
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            _layer.GetTile(x, y);
        }
        sw.Stop();

        Assert.IsTrue(sw.ElapsedMilliseconds < 50,
            $"Access time too slow: {sw.ElapsedMilliseconds}ms");
    }
}