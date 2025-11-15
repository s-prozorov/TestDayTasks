using MapSystem.Core.SurfaceLayer;

namespace MapSystem.Core.Tests;

public class супер
{
    [Fact]
    public void CanPlace_Flat_ReturnsTrue()
    {
        var canPlace = TileType.Flat.CanPlace();

        Assert.True(canPlace);
    }

    [Fact]
    public void CanPlace_Mountain_ReturnsFalse()
    {
        var canPlace = TileType.Mountain.CanPlace();

        Assert.False(canPlace);
    }

    [Fact]
    public void CanPlace_InvalidType_ThrowsException()
    {
        var invalidType = (TileType)99;

        Assert.Throws<ArgumentException>(() => invalidType.CanPlace());
    }
}