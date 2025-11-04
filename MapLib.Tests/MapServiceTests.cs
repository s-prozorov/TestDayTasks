using MapLib.Core.Models;
using MapLib.Core.Models.TileModel;
using MapLib.Services;
using Xunit;

namespace MapLib.Tests;

public class MapServiceTests
{
    private readonly MapService mapService;

    public MapServiceTests()
    {
        MapProvider mapProvider = new MapProvider();
        mapService = new MapService(mapProvider);
    }
    
    [Fact]
    public void GetTile_ValidCoordinates_ReturnsTile()
    {
        // Act
        var tile = mapService.GetTile(0, 0);
        
        Assert.NotNull(tile);
        Assert.InRange(tile.X, 0, 999);
        Assert.InRange(tile.Y, 0, 999);
        Assert.IsType<TileType>(tile.Type);
        Assert.InRange(tile.TerritoryId, 0, 86);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(500, 500)]
    [InlineData(999, 999)]
    public void GetTile_ValidCoordinates_ReturnsCorrectTile(int x, int y)
    {
        // Act
        var tile = mapService.GetTile(x, y);
        
        // Assert
        Assert.Equal(x, tile.X);
        Assert.Equal(y, tile.Y);
    }
    
    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(1000, 0)]
    [InlineData(0, 1000)]
    [InlineData(1000, 1000)]
    public void GetTile_InvalidCoordinates_ThrowsException(int x, int y)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => mapService.GetTile(x, y));
    }
    
    [Fact]
    public void GetTileType_ConsistentWithGetTile()
    {
        // Arrange
        int x = 100, y = 200;

        // Act
        var tileTypeFromMethod = mapService.GetTileType(x, y);
        var tileTypeFromTile = mapService.GetTile(x, y).Type;

        // Assert
        Assert.Equal(tileTypeFromTile, tileTypeFromMethod);
    }
    
    [Fact]
    public void GetTerritoryId_ValidCoordinates_ReturnsValidId()
    {
        // Act
        var territoryId = mapService.GetTerritoryId(50, 50);

        // Assert
        Assert.InRange(territoryId, 0, 86);
    }
    
    [Fact]
    public void GetTerritoryId_ConsistentWithGetTile()
    {
        // Arrange
        int x = 300, y = 400;

        // Act
        var territoryIdFromMethod = mapService.GetTerritoryId(x, y);
        var territoryIdFromTile = mapService.GetTile(x, y).TerritoryId;

        // Assert
        Assert.Equal(territoryIdFromTile, territoryIdFromMethod);
    }
    
    [Fact]
    public void GetTerritoryInfo_ValidId_ReturnsTerritory()
    {
        // Arrange
        int validId = 42; // Должен существовать тк у нас 87 территорий

        // Act
        var territory = mapService.GetTerritoryInfo(validId);

        // Assert
        Assert.NotNull(territory);
        Assert.Equal(validId, territory.Id);
        Assert.NotNull(territory.BordersInfo);
        Assert.NotNull(territory.Tiles);
        Assert.NotEmpty(territory.Tiles);
    }
    
    [Fact]
    public void GetTerritoryInfo_InvalidId_ReturnsNull()
    {
        // Arrange
        int invalidId = 87; // Больше 86

        // Act
        var territory = mapService.GetTerritoryInfo(invalidId);

        // Assert
        Assert.Null(territory);
    }
    
    [Fact]
    public void GetTerritoryInfo_NegativeId_ReturnsNull()
    {
        // Act
        var territory = mapService.GetTerritoryInfo(-1);

        // Assert
        Assert.Null(territory);
    }
    
    [Fact]
    public void Territory_Tiles_HaveConsistentCoordinates()
    {
        // Arrange
        int territoryId = 25;

        // Act
        var territory = mapService.GetTerritoryInfo(territoryId);

        // Assert
        Assert.NotNull(territory);
        foreach (var tile in territory.Tiles)
        {
            Assert.Equal(territoryId, tile.TerritoryId);
            Assert.InRange(tile.X, 0, 999);
            Assert.InRange(tile.Y, 0, 999);
        }
    }
    
    
    [Fact]
    public void AllTiles_HaveValidTerritoryIds()
    {
        // random samples
        var testPoints = new[] { (0, 0), (500, 500), (888, 888), (123, 456), (789, 321) };

        foreach (var (x, y) in testPoints)
        {
            var territoryId = mapService.GetTerritoryId(x, y);
            Assert.InRange(territoryId, 0, 86);
        }
    }
    
    [Fact]
    public void AllTiles_HaveValidTileTypes()
    {
        // random samples
        var testPoints = new[] { (0, 0), (250, 750), (888, 888) };

        foreach (var (x, y) in testPoints)
        {
            var tileType = mapService.GetTileType(x, y);
            Assert.True(tileType == TileType.Plane || tileType == TileType.Mountain);
        }
    }
    
}