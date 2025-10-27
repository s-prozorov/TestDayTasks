using Xunit;

namespace MapObjects.Tests;

public class CoordinateConverterTests
{
    [Fact]
    public void ToGeoCoordinates_Origin_ReturnsBaseCoordinates()
    {
        var (lat, lon) = CoordinateConverter.ToGeoCoordinates(0, 0);

        Assert.Equal(0.0, lat);
        Assert.Equal(0.0, lon);
    }

    [Fact]
    public void ToGeoCoordinates_PositiveCoordinates_ReturnsCorrectGeoCoordinates()
    {
        var (lat, lon) = CoordinateConverter.ToGeoCoordinates(100, 200);

        Assert.Equal(0.002, lat, 6);
        Assert.Equal(0.001, lon, 6);
    }

    [Fact]
    public void ToGeoCoordinates_NegativeCoordinates_ReturnsCorrectGeoCoordinates()
    {
        var (lat, lon) = CoordinateConverter.ToGeoCoordinates(-100, -200);

        Assert.Equal(-0.002, lat, 6);
        Assert.Equal(-0.001, lon, 6);
    }

    [Fact]
    public void FromGeoCoordinates_BaseCoordinates_ReturnsOrigin()
    {
        var (x, y) = CoordinateConverter.FromGeoCoordinates(0.0, 0.0);

        Assert.Equal(0, x);
        Assert.Equal(0, y);
    }

    [Fact]
    public void FromGeoCoordinates_PositiveGeoCoordinates_ReturnsCorrectCoordinates()
    {
        var (x, y) = CoordinateConverter.FromGeoCoordinates(0.002, 0.001);

        Assert.Equal(100, x);
        Assert.Equal(200, y);
    }

    [Fact]
    public void FromGeoCoordinates_NegativeGeoCoordinates_ReturnsCorrectCoordinates()
    {
        var (x, y) = CoordinateConverter.FromGeoCoordinates(-0.002, -0.001);

        Assert.Equal(-100, x);
        Assert.Equal(-200, y);
    }

    [Fact]
    public void RoundTrip_ToAndFromGeo_PreservesCoordinates()
    {
        int originalX = 500;
        int originalY = 750;

        var (lat, lon) = CoordinateConverter.ToGeoCoordinates(originalX, originalY);
        var (x, y) = CoordinateConverter.FromGeoCoordinates(lat, lon);

        Assert.Equal(originalX, x);
        Assert.Equal(originalY, y);
    }

    [Fact]
    public void RoundTrip_LargeMap_PreservesCoordinates()
    {
        int originalX = 999;
        int originalY = 999;

        var (lat, lon) = CoordinateConverter.ToGeoCoordinates(originalX, originalY);
        var (x, y) = CoordinateConverter.FromGeoCoordinates(lat, lon);

        Assert.Equal(originalX, x);
        Assert.Equal(originalY, y);
    }
}

