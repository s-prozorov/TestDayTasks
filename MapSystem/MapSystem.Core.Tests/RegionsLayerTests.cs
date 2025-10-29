using MapSystem.Core.RegionLayer;

namespace MapSystem.Core.Tests;

public sealed class RegionsLayerTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesLayer()
    {
        var layer = RegionsLayer.Create(100, 100, 25);


        Assert.NotNull(layer);
        Assert.Equal(100, layer.Width);
        Assert.Equal(100, layer.Height);
    }

    [Theory]
    [InlineData(0, 100, 25)]
    [InlineData(100, 0, 25)]
    [InlineData(100, 100, 0)]
    [InlineData(-1, 100, 25)]
    [InlineData(100, -1, 25)]
    [InlineData(100, 100, -1)]
    public void Create_WithInvalidParameters_ThrowsArgumentOutOfRangeException(
        int width,
        int height,
        int targetRegionCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RegionsLayer.Create(width, height, targetRegionCount));
    }

    [Fact]
    public void Create_WithNonDivisibleDimensions_CreatesValidLayer()
    {
        var layer = RegionsLayer.Create(100, 100, 30);

        Assert.NotNull(layer);
        Assert.Equal(100, layer.Width);
        Assert.Equal(100, layer.Height);

        var allTilesHaveRegion = true;
        for (var y = 0; y < 100; y++)
        for (var x = 0; x < 100; x++)
        {
            var regionId = layer.GetRegionId(x, y);
            allTilesHaveRegion &= layer.GetRegion(regionId) != null;
        }

        Assert.True(allTilesHaveRegion);
    }

    [Fact]
    public void Create_GeneratesRegionsWithEqualArea()
    {
        var layer = RegionsLayer.Create(100, 100, 100);
        var regionTileCounts = new Dictionary<ushort, int>();

        for (var y = 0; y < 100; y++)
        for (var x = 0; x < 100; x++)
        {
            var regionId = layer.GetRegionId(x, y);
            regionTileCounts.TryGetValue(regionId, out var count);
            regionTileCounts[regionId] = count + 1;
        }

        var expectedArea = 100 * 100 / 100;
        Assert.Equal(100, regionTileCounts.Count);
        Assert.All(regionTileCounts.Values, count => Assert.Equal(expectedArea, count));
    }

    [Fact]
    public void Create_AllTilesBelongToRegion()
    {
        var layer = RegionsLayer.Create(50, 50, 25);
        var allRegionIds = new HashSet<ushort>();

        for (var y = 0; y < 50; y++)
        for (var x = 0; x < 50; x++)
        {
            var regionId = layer.GetRegionId(x, y);
            allRegionIds.Add(regionId);
        }

        Assert.Equal(25, allRegionIds.Count);
    }

    [Fact]
    public void GetRegionId_WithValidCoordinates_ReturnsRegionId()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var regionId = layer.GetRegionId(5, 5);

        Assert.InRange(regionId, 0, 99);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(100, 0)]
    [InlineData(0, 100)]
    public void GetRegionId_WithInvalidCoordinates_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetRegionId(x, y));
    }

    [Fact]
    public void GetRegionId_ReturnsConsistentResults()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var regionId1 = layer.GetRegionId(15, 25);
        var regionId2 = layer.GetRegionId(15, 25);

        Assert.Equal(regionId1, regionId2);
    }

    [Fact]
    public void GetRegion_WithValidId_ReturnsRegion()
    {
        var layer = RegionsLayer.Create(100, 100, 100);
        var regionId = layer.GetRegionId(5, 5);

        var region = layer.GetRegion(regionId);

        Assert.NotNull(region);
        Assert.Equal(regionId, region.Id);
        Assert.False(string.IsNullOrEmpty(region.Name));
    }

    [Fact]
    public void GetRegion_WithInvalidId_ThrowsArgumentException()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var exception = Assert.Throws<ArgumentException>(() => layer.GetRegion(9999));
        Assert.Contains("не найден", exception.Message);
    }

    [Fact]
    public void GetRegion_ReturnsCorrectNameFormat()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var region = layer.GetRegion(0);


        Assert.Matches(@"Region \[\d+,\d+\]", region.Name);
    }

    [Fact]
    public void IsInRegion_WhenTileBelongsToRegion_ReturnsTrue()
    {
        var layer = RegionsLayer.Create(100, 100, 100);
        var regionId = layer.GetRegionId(5, 5);

        var result = layer.IsInRegion(5, 5, regionId);

        Assert.True(result);
    }

    [Fact]
    public void IsInRegion_WhenTileNotBelongsToRegion_ReturnsFalse()
    {
        var layer = RegionsLayer.Create(100, 100, 100);
        var regionId = layer.GetRegionId(5, 5);

        var result = layer.IsInRegion(95, 95, regionId);

        Assert.False(result);
    }

    [Fact]
    public void IsInRegion_WithNonExistentRegionId_ReturnsFalse()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var result = layer.IsInRegion(5, 5, 9999);

        Assert.False(result);
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(100, 0, 0)]
    [InlineData(0, 100, 0)]
    public void IsInRegion_WithInvalidCoordinates_ThrowsArgumentOutOfRangeException(
        int x,
        int y,
        ushort regionId)
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.IsInRegion(x, y, regionId));
    }

    [Fact]
    public void GetRegionsInArea_WithSingleRegion_ReturnsSingleRegion()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var regions = layer.GetRegionsInArea(11, 11, 5, 5);

        Assert.Single(regions);
    }

    [Fact]
    public void GetRegionsInArea_WithMultipleRegions_ReturnsAllRegions()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var regions = layer.GetRegionsInArea(8, 8, 4, 4);

        Assert.Equal(4, regions.Count);
    }

    [Fact]
    public void GetRegionsInArea_WithEntireMap_ReturnsAllRegions()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var regions = layer.GetRegionsInArea(0, 0, 100, 100);

        Assert.Equal(100, regions.Count);
    }

    [Theory]
    [InlineData(-1, 0, 10, 10)]
    [InlineData(0, -1, 10, 10)]
    [InlineData(0, 0, 0, 10)]
    [InlineData(0, 0, 10, 0)]
    [InlineData(0, 0, -1, 10)]
    [InlineData(0, 0, 10, -1)]
    public void GetRegionsInArea_WithInvalidArea_ThrowsArgumentOutOfRangeException(
        int x,
        int y,
        int width,
        int height)
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            layer.GetRegionsInArea(x, y, width, height));
    }

    [Theory]
    [InlineData(95, 0, 10, 10)]
    [InlineData(0, 95, 10, 10)]
    public void GetRegionsInArea_WhenAreaExceedsBounds_ThrowsArgumentOutOfRangeException(
        int x,
        int y,
        int width,
        int height)
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            layer.GetRegionsInArea(x, y, width, height));

        Assert.Contains("выходит за", exception.Message);
        Assert.Contains("границу карты", exception.Message);
    }

    [Fact]
    public void GetRegionsInArea_ReturnsDistinctRegions()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var regions = layer.GetRegionsInArea(0, 0, 50, 50);

        var distinctRegionIds = regions.Select(r => r.Id).Distinct().Count();
        Assert.Equal(regions.Count, distinctRegionIds);
    }

    [Fact]
    public void GetRegionsInArea_OnRegionBoundary_ReturnsCorrectRegions()
    {
        var layer = RegionsLayer.Create(100, 100, 100);

        var regions = layer.GetRegionsInArea(9, 9, 2, 2);

        Assert.Equal(4, regions.Count);
    }

    [Fact]
    public void Region_SetAndGetMetadata_Works()
    {
        var layer = RegionsLayer.Create(100, 100, 25);
        var region = layer.GetRegion(0);

        region.SetMetadata("Population", 100000);
        region.SetMetadata("Owner", "Player1");

        Assert.Equal(100000, region.GetMetadata<int>("Population"));
        Assert.Equal("Player1", region.GetMetadata<string>("Owner"));
    }

    [Fact]
    public void Region_TryGetMetadata_Works()
    {
        var layer = RegionsLayer.Create(100, 100, 25);
        var region = layer.GetRegion(0);
        region.SetMetadata("Population", 100000);

        Assert.True(region.TryGetMetadata("Population", out int population));
        Assert.Equal(100000, population);

        Assert.False(region.TryGetMetadata("NonExistent", out int _));
    }

    [Fact]
    public void Region_HasMetadata_Works()
    {
        var layer = RegionsLayer.Create(100, 100, 25);
        var region = layer.GetRegion(0);
        region.SetMetadata("Key", "Value");

        Assert.True(region.HasMetadata("Key"));
        Assert.False(region.HasMetadata("NonExistent"));
    }

    [Fact]
    public void Region_RemoveMetadata_Works()
    {
        var layer = RegionsLayer.Create(100, 100, 25);
        var region = layer.GetRegion(0);
        region.SetMetadata("Key", "Value");

        var removed = region.RemoveMetadata("Key");

        Assert.True(removed);
        Assert.False(region.HasMetadata("Key"));
    }
}