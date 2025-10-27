using Xunit;

namespace MapObjects.Tests;

public class MapObjectTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesObject()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.Equal("obj1", obj.Id);
        Assert.Equal(10, obj.X);
        Assert.Equal(20, obj.Y);
        Assert.Equal(5, obj.Width);
        Assert.Equal(3, obj.Height);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_InvalidId_ThrowsArgumentException(string id)
    {
        Assert.Throws<ArgumentException>(() => new MapObject(id, 0, 0, 1, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidWidth_ThrowsArgumentException(int width)
    {
        Assert.Throws<ArgumentException>(() => new MapObject("obj1", 0, 0, width, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidHeight_ThrowsArgumentException(int height)
    {
        Assert.Throws<ArgumentException>(() => new MapObject("obj1", 0, 0, 1, height));
    }

    [Fact]
    public void UpdatePosition_ValidCoordinates_UpdatesPosition()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        obj.UpdatePosition(15, 25);

        Assert.Equal(15, obj.X);
        Assert.Equal(25, obj.Y);
    }

    [Fact]
    public void UpdateSize_ValidSize_UpdatesSize()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        obj.UpdateSize(7, 4);

        Assert.Equal(7, obj.Width);
        Assert.Equal(4, obj.Height);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void UpdateSize_InvalidSize_ThrowsArgumentException(int width, int height)
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.Throws<ArgumentException>(() => obj.UpdateSize(width, height));
    }

    [Theory]
    [InlineData(10, 20, true)]
    [InlineData(14, 22, true)]
    [InlineData(14, 20, true)]
    [InlineData(10, 22, true)]
    [InlineData(9, 20, false)]
    [InlineData(15, 20, false)]
    [InlineData(10, 19, false)]
    [InlineData(10, 23, false)]
    public void ContainsPoint_VariousPoints_ReturnsCorrectResult(int x, int y, bool expected)
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        var result = obj.ContainsPoint(x, y);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IntersectsArea_OverlappingArea_ReturnsTrue()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.True(obj.IntersectsArea(12, 21, 5, 3));
    }

    [Fact]
    public void IntersectsArea_FullyContainedArea_ReturnsTrue()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.True(obj.IntersectsArea(11, 21, 2, 1));
    }

    [Fact]
    public void IntersectsArea_ContainingArea_ReturnsTrue()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.True(obj.IntersectsArea(5, 15, 20, 20));
    }

    [Fact]
    public void IntersectsArea_NonOverlappingArea_ReturnsFalse()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.False(obj.IntersectsArea(0, 0, 5, 5));
    }

    [Fact]
    public void IntersectsArea_AdjacentArea_ReturnsFalse()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.False(obj.IntersectsArea(15, 20, 5, 3));
    }

    [Fact]
    public void IsFullyInArea_FullyContained_ReturnsTrue()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.True(obj.IsFullyInArea(5, 15, 20, 20));
    }

    [Fact]
    public void IsFullyInArea_ExactMatch_ReturnsTrue()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.True(obj.IsFullyInArea(10, 20, 5, 3));
    }

    [Fact]
    public void IsFullyInArea_PartiallyOutside_ReturnsFalse()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.False(obj.IsFullyInArea(11, 20, 5, 3));
    }

    [Fact]
    public void IsFullyInArea_CompletelyOutside_ReturnsFalse()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        Assert.False(obj.IsFullyInArea(0, 0, 5, 5));
    }
}

