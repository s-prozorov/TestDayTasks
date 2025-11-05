using Application.Features.Map.Enums;
using Application.Features.Map.Models;
using Application.Features.Map.Services;

namespace Application.Tests.Features.Map.Services;

public class MapServiceTests
{
    [Fact]
    public void GenerateMap_ShouldInitializeMap()
    {
        // Arrange
        var service = new MapService();

        // Act
        service.GenerateMap();

        // Assert
        Assert.Equal(1_000_000, service.Map.Length);
        Assert.Equal(1_000_000, service.Map.Length);
        Assert.NotEmpty(service.Territories);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 200)]
    [InlineData(500, 500)]
    [InlineData(999, 999)]
    public void GetTile_ShouldReturnTile(short x, short y)
    {
        // Arrange
        var service = new MapService();
        service.GenerateMap();

        // Act
        var tile = service.GetTile(x, y);

        // Assert
        Assert.NotNull(tile);
        Assert.NotNull(tile.Type);
        Assert.True(tile.TerritoryId >= 0);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 4)]
    [InlineData(6, 8)]
    [InlineData(1, 1)]
    [InlineData(100, 200)]
    public void GetTileType_ShouldReturnType(short x, short y)
    {
        // Arrange
        var service = new MapService();
        service.GenerateMap();

        // Act
        var tileType = service.GetTileType(x, y);

        // Assert
        Assert.True(tileType == TileTypeEnum.Mountain || tileType == TileTypeEnum.Plain);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 200)]
    [InlineData(500, 500)]
    [InlineData(999, 999)]
    public void GetTerritoryId_ShouldReturnId(short x, short y)
    {
        // Arrange
        var service = new MapService();
        service.GenerateMap();

        // Act
        var territoryId = service.GetTerritoryId(x, y);

        // Assert
        Assert.True(territoryId >= 0);
    }

    [Fact]
    public void GetTerritoryInfo_ShouldReturnTerritoryInfo()
    {
        // Arrange
        var service = new MapService();
        service.GenerateMap();
        var territoryId = service.Territories.Keys.First();

        // Act
        var territoryInfo = service.GetTerritoryInfo(territoryId);

        // Assert
        Assert.NotNull(territoryInfo);
        Assert.Equal(territoryId, territoryInfo.Id);
        Assert.NotNull(territoryInfo.Tiles);
    }

    [Fact]
    public void GetTerritoryInfo_ShouldThrowExceptionWhenTerritoryNotFound()
    {
        // Arrange
        var service = new MapService();
        service.GenerateMap();
        var nonExistentTerritoryId = (byte)255;

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => service.GetTerritoryInfo(nonExistentTerritoryId));
    }
}

