using Xunit;

namespace MapSurface.Tests;

public class TilePropertiesTests
{
    [Fact]
    public void GetProperties_PlainTile_CanPlaceObject()
    {
        var properties = TileProperties.GetProperties(TileType.Plain);

        Assert.True(properties.CanPlaceObject);
    }

    [Fact]
    public void GetProperties_MountainTile_CannotPlaceObject()
    {
        var properties = TileProperties.GetProperties(TileType.Mountain);

        Assert.False(properties.CanPlaceObject);
    }

    [Fact]
    public void GetProperties_SameTileType_ReturnsSameInstance()
    {
        var properties1 = TileProperties.GetProperties(TileType.Plain);
        var properties2 = TileProperties.GetProperties(TileType.Plain);

        Assert.Same(properties1, properties2);
    }

    [Fact]
    public void GetProperties_InvalidTileType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TileProperties.GetProperties((TileType)99));
    }
}

