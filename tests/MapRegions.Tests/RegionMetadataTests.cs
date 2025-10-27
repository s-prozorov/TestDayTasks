using Xunit;

namespace MapRegions.Tests;

public class RegionMetadataTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesMetadata()
    {
        var metadata = new RegionMetadata(1, "Test Region");

        Assert.Equal(1, metadata.Id);
        Assert.Equal("Test Region", metadata.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_InvalidName_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => new RegionMetadata(1, name));
    }
}

