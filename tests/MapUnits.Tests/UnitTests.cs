using MapUnits.Core;
using Xunit;

namespace MapUnits.Tests;

public class UnitTests
{
    [Fact]
    public void Constructor_CreatesUnit()
    {
        var unit = new Unit("u1", 10.5f, 20.3f, 5.0f);

        Assert.Equal("u1", unit.Id);
        Assert.Equal(10.5f, unit.X);
        Assert.Equal(20.3f, unit.Y);
        Assert.Equal(5.0f, unit.BaseSpeed);
    }

    [Fact]
    public void GetGridCell_ReturnsCorrectCell()
    {
        var unit = new Unit("u1", 10.8f, 20.3f);

        var (x, y) = unit.GetGridCell();

        Assert.Equal(10, x);
        Assert.Equal(20, y);
    }

    [Fact]
    public void GetGridCell_NegativeCoords_ReturnsCorrectCell()
    {
        var unit = new Unit("u1", -5.2f, -3.7f);

        var (x, y) = unit.GetGridCell();

        Assert.Equal(-5, x);
        Assert.Equal(-3, y);
    }
}

