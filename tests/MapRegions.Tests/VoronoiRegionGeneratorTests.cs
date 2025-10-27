using Xunit;

namespace MapRegions.Tests;

public class VoronoiRegionGeneratorTests
{
    [Fact]
    public void Generate_ValidParameters_GeneratesRegions()
    {
        var generator = new VoronoiRegionGenerator(42);

        var regions = generator.Generate(100, 100, 10);

        Assert.Equal(10000, regions.Length);
    }

    [Fact]
    public void Generate_ValidParameters_AllTilesAssigned()
    {
        var generator = new VoronoiRegionGenerator(42);

        var regions = generator.Generate(50, 50, 5);

        foreach (var region in regions)
        {
            Assert.InRange(region, 0, 4);
        }
    }

    [Fact]
    public void Generate_ValidParameters_AllRegionsPresent()
    {
        var generator = new VoronoiRegionGenerator(42);

        var regions = generator.Generate(100, 100, 10);

        var uniqueRegions = regions.Distinct().OrderBy(r => r).ToArray();
        Assert.Equal(10, uniqueRegions.Length);
        for (ushort i = 0; i < 10; i++)
        {
            Assert.Contains(i, uniqueRegions);
        }
    }

    [Fact]
    public void Generate_ValidParameters_AllRegionsHaveReasonableSizes()
    {
        var generator = new VoronoiRegionGenerator(42);
        var regions = generator.Generate(100, 100, 10);

        var regionCounts = new int[10];
        foreach (var region in regions)
        {
            regionCounts[region]++;
        }

        foreach (var count in regionCounts)
        {
            Assert.InRange(count, 1, 10000);
        }
    }

    [Theory]
    [InlineData(0, 100, 10)]
    [InlineData(-1, 100, 10)]
    [InlineData(100, 0, 10)]
    [InlineData(100, -1, 10)]
    [InlineData(100, 100, 0)]
    [InlineData(100, 100, -1)]
    public void Generate_InvalidParameters_ThrowsArgumentException(int width, int height, int regionCount)
    {
        var generator = new VoronoiRegionGenerator();

        Assert.Throws<ArgumentException>(() => generator.Generate(width, height, regionCount));
    }

    [Fact]
    public void Generate_SameSeed_GeneratesSameRegions()
    {
        var generator1 = new VoronoiRegionGenerator(123);
        var generator2 = new VoronoiRegionGenerator(123);

        var regions1 = generator1.Generate(50, 50, 5);
        var regions2 = generator2.Generate(50, 50, 5);

        Assert.Equal(regions1, regions2);
    }

    [Fact]
    public void Generate_DifferentSeeds_GeneratesDifferentRegions()
    {
        var generator1 = new VoronoiRegionGenerator(123);
        var generator2 = new VoronoiRegionGenerator(456);

        var regions1 = generator1.Generate(50, 50, 5);
        var regions2 = generator2.Generate(50, 50, 5);

        Assert.NotEqual(regions1, regions2);
    }
}

