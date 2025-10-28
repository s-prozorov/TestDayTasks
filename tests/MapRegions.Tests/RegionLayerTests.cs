using Xunit;

namespace MapRegions.Tests;

public class RegionLayerTests
{
    [Fact]
    public void Create_ValidParameters_CreatesLayer()
    {
        var generator = new VoronoiRegionGenerator(42);

        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        Assert.Equal(100, layer.Width);
        Assert.Equal(100, layer.Height);
        Assert.Equal(10, layer.RegionCount);
    }

    [Theory]
    [InlineData(0, 100, 10)]
    [InlineData(-1, 100, 10)]
    [InlineData(100, 0, 10)]
    [InlineData(100, -1, 10)]
    [InlineData(100, 100, 0)]
    [InlineData(100, 100, -1)]
    public void Create_InvalidParameters_ThrowsArgumentException(int width, int height, int regionCount)
    {
        var generator = new VoronoiRegionGenerator();

        Assert.Throws<ArgumentException>(() => 
            RegionLayer.Create(width, height, regionCount, generator, id => $"Region {id}"));
    }

    [Fact]
    public void Create_NullGenerator_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RegionLayer.Create(100, 100, 10, null!, id => $"Region {id}"));
    }

    [Fact]
    public void Create_NullNameGenerator_ThrowsArgumentNullException()
    {
        var generator = new VoronoiRegionGenerator();

        Assert.Throws<ArgumentNullException>(() =>
            RegionLayer.Create(100, 100, 10, generator, null!));
    }

    [Fact]
    public void GetRegionId_ValidCoordinates_ReturnsRegionId()
    {
        var generator = new VoronoiRegionGenerator(42);
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        var regionId = layer.GetRegionId(50, 50);

        Assert.InRange(regionId, (ushort)0, (ushort)9);
    }

    [Theory]
    [InlineData(-1, 50)]
    [InlineData(100, 50)]
    [InlineData(50, -1)]
    [InlineData(50, 100)]
    public void GetRegionId_OutOfBounds_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        var generator = new VoronoiRegionGenerator();
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetRegionId(x, y));
    }

    [Fact]
    public void GetRegionMetadata_ValidRegionId_ReturnsMetadata()
    {
        var generator = new VoronoiRegionGenerator();
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        var metadata = layer.GetRegionMetadata(5);

        Assert.Equal(5, metadata.Id);
        Assert.Equal("Region 5", metadata.Name);
    }

    [Fact]
    public void GetRegionMetadata_InvalidRegionId_ThrowsArgumentException()
    {
        var generator = new VoronoiRegionGenerator();
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        Assert.Throws<ArgumentException>(() => layer.GetRegionMetadata(99));
    }

    [Fact]
    public void TilebelongsToRegion_TileBelongsToRegion_ReturnsTrue()
    {
        var generator = new VoronoiRegionGenerator(42);
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        var regionId = layer.GetRegionId(50, 50);
        var belongs = layer.TilebelongsToRegion(50, 50, regionId);

        Assert.True(belongs);
    }

    [Fact]
    public void TilebelongsToRegion_TileDoesNotBelongToRegion_ReturnsFalse()
    {
        var generator = new VoronoiRegionGenerator(42);
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        var regionId = layer.GetRegionId(50, 50);
        var otherRegionId = (ushort)((regionId + 1) % 10);
        var belongs = layer.TilebelongsToRegion(50, 50, otherRegionId);

        Assert.False(belongs);
    }

    [Theory]
    [InlineData(-1, 50)]
    [InlineData(100, 50)]
    [InlineData(50, -1)]
    [InlineData(50, 100)]
    public void TilebelongsToRegion_OutOfBounds_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        var generator = new VoronoiRegionGenerator();
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.TilebelongsToRegion(x, y, 0));
    }

    [Fact]
    public void GetRegionsInArea_ValidArea_ReturnsRegions()
    {
        var generator = new VoronoiRegionGenerator(42);
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        var regions = layer.GetRegionsInArea(40, 40, 20, 20);

        Assert.NotEmpty(regions);
        Assert.All(regions, r => Assert.InRange(r, (ushort)0, (ushort)9));
    }

    [Fact]
    public void GetRegionsInArea_SmallArea_MayReturnSingleRegion()
    {
        var generator = new VoronoiRegionGenerator(42);
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        var regions = layer.GetRegionsInArea(50, 50, 1, 1);

        Assert.Single(regions);
    }

    [Fact]
    public void GetRegionsInArea_LargeArea_ReturnsMultipleRegions()
    {
        var generator = new VoronoiRegionGenerator(42);
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        var regions = layer.GetRegionsInArea(0, 0, 100, 100);

        Assert.Equal(10, regions.Count);
    }

    [Theory]
    [InlineData(-1, 0, 10, 10)]
    [InlineData(0, -1, 10, 10)]
    [InlineData(91, 0, 10, 10)]
    [InlineData(0, 91, 10, 10)]
    [InlineData(0, 0, 0, 10)]
    [InlineData(0, 0, 10, 0)]
    [InlineData(0, 0, -1, 10)]
    [InlineData(0, 0, 10, -1)]
    public void GetRegionsInArea_InvalidArea_ThrowsException(int x, int y, int width, int height)
    {
        var generator = new VoronoiRegionGenerator();
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        Assert.ThrowsAny<Exception>(() => layer.GetRegionsInArea(x, y, width, height));
    }

    [Fact]
    public void RegionGeneration_AllRegionsExist()
    {
        var generator = new VoronoiRegionGenerator(42);
        var layer = RegionLayer.Create(1000, 1000, 100, generator, id => $"Region {id}");

        var regionCounts = new Dictionary<ushort, int>();
        for (int y = 0; y < 1000; y++)
        {
            for (int x = 0; x < 1000; x++)
            {
                var regionId = layer.GetRegionId(x, y);
                regionCounts.TryGetValue(regionId, out int count);
                regionCounts[regionId] = count + 1;
            }
        }

        Assert.Equal(100, regionCounts.Count);
        foreach (var kvp in regionCounts)
        {
            Assert.InRange(kvp.Value, 1, 1000000);
        }
    }

    [Fact]
    public void RegionGeneration_AllTilesBelongToSomeRegion()
    {
        var generator = new VoronoiRegionGenerator(42);
        var layer = RegionLayer.Create(100, 100, 10, generator, id => $"Region {id}");

        for (int y = 0; y < 100; y++)
        {
            for (int x = 0; x < 100; x++)
            {
                var regionId = layer.GetRegionId(x, y);
                Assert.InRange(regionId, (ushort)0, (ushort)9);
            }
        }
    }
}

