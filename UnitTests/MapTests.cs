using MapLib;
using MapLib.Enum;
using MapLib.Models;

namespace UnitTests;

public class MapTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesEmptyMap()
    {
        // Act
        var map = new Map();
        
        // Assert
        Assert.Equal(TileType.Plain, map.GetTileType(0, 0));
        Assert.Equal(0, map.GetTerritoryId(0, 0));
    }

    #endregion

    #region GetTile Tests

    [Fact]
    public void GetTile_ValidCoordinates_ReturnsPackedTile()
    {
        // Arrange
        var map = new Map();
        map.SetTile(10, 20, TileType.Plain, 5);
        
        // Act
        long packedTile = map.GetTile(10, 20);
        
        // Assert
        Assert.Equal(TileType.Plain, map.GetTileTypeFromPacked(packedTile));
        Assert.Equal(5, map.GetTerritoryIdFromPacked(packedTile));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(1000, 0)]
    [InlineData(0, 1000)]
    public void GetTile_InvalidCoordinates_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        // Arrange
        var map = new Map();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetTile(x, y));
    }

    [Fact]
    public void GetTile_BoundaryCoordinates_ReturnsValidTile()
    {
        // Arrange
        var map = new Map();
        map.SetTile(0, 0, TileType.Plain, 1);
        map.SetTile(999, 999, TileType.Mountain, 2);
        
        // Act
        long tile1 = map.GetTile(0, 0);
        long tile2 = map.GetTile(999, 999);
        
        // Assert
        Assert.Equal(TileType.Plain, map.GetTileTypeFromPacked(tile1));
        Assert.Equal(1, map.GetTerritoryIdFromPacked(tile1));
        Assert.Equal(TileType.Mountain, map.GetTileTypeFromPacked(tile2));
        Assert.Equal(2, map.GetTerritoryIdFromPacked(tile2));
    }

    #endregion

    #region GetTileType and GetTerritoryId Tests

    [Fact]
    public void GetTileType_ValidCoordinates_ReturnsTileType()
    {
        // Arrange
        var map = new Map();
        map.SetTile(100, 200, TileType.Mountain, 10);
        
        // Act & Assert
        Assert.Equal(TileType.Mountain, map.GetTileType(100, 200));
        Assert.Equal(10, map.GetTerritoryId(100, 200));
    }

    #endregion

    #region SetTile Tests

    [Fact]
    public void SetTile_ValidParameters_SetsTileCorrectly()
    {
        // Arrange
        var map = new Map();
        
        // Act
        map.SetTile(500, 600, TileType.Mountain, 25);
        
        // Assert
        Assert.Equal(TileType.Mountain, map.GetTileType(500, 600));
        Assert.Equal(25, map.GetTerritoryId(500, 600));
    }

    [Theory]
    [InlineData(-1, 0, TileType.Plain, 1)]
    [InlineData(1000, 0, TileType.Plain, 1)]
    public void SetTile_InvalidCoordinates_ThrowsArgumentOutOfRangeException(
        int x, int y, TileType type, int territoryId)
    {
        // Arrange
        var map = new Map();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            map.SetTile(x, y, type, territoryId));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(87)]
    public void SetTile_InvalidTerritoryId_ThrowsArgumentOutOfRangeException(int territoryId)
    {
        // Arrange
        var map = new Map();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            map.SetTile(0, 0, TileType.Plain, territoryId));
    }

    [Fact]
    public void SetTile_OverwriteExistingTile_UpdatesCorrectly()
    {
        // Arrange
        var map = new Map();
        map.SetTile(10, 10, TileType.Plain, 5);
        
        // Act
        map.SetTile(10, 10, TileType.Mountain, 10);
        
        // Assert
        Assert.Equal(TileType.Mountain, map.GetTileType(10, 10));
        Assert.Equal(10, map.GetTerritoryId(10, 10));
    }

    #endregion

    #region Packing/Unpacking Tests

    [Fact]
    public void PackedTile_Format_CorrectBitLayout()
    {
        // Arrange
        var map = new Map();
        map.SetTile(0, 0, TileType.Mountain, 52);
        
        // Act
        long packedTile = map.GetTile(0, 0);
        
        // Assert
        TileType unpackedType = map.GetTileTypeFromPacked(packedTile);
        int unpackedTerritoryId = map.GetTerritoryIdFromPacked(packedTile);
        
        Assert.Equal(TileType.Mountain, unpackedType);
        Assert.Equal(52, unpackedTerritoryId);
    }

    [Fact]
    public void Tile_PackUnpack_RoundTrip()
    {
        // Arrange
        var originalTile = new Tile(100, 200, TileType.Mountain, 42);
        
        // Act
        long packed = originalTile.Pack();
        var unpacked = Tile.Unpack(packed, 100, 200);
        
        // Assert
        Assert.Equal(originalTile.X, unpacked.X);
        Assert.Equal(originalTile.Y, unpacked.Y);
        Assert.Equal(originalTile.Type, unpacked.Type);
        Assert.Equal(originalTile.TerritoryId, unpacked.TerritoryId);
    }

    #endregion

    #region GetTerritoryInfo Tests

    [Fact]
    public void GetTerritoryInfo_ExistingId_ReturnsTerritoryInfo()
    {
        // Arrange
        var map = new Map();
        var territories = new[]
        {
            new TerritoryInfo(0, -1,"Territory 0"),
            new TerritoryInfo(1, 100,"Territory 1")
        };
        map.InitializeTerritories(territories);
        
        // Act
        var info = map.GetTerritoryInfo(1);
        
        // Assert
        Assert.Equal(1, info.Id);
        Assert.Equal("Territory 1", info.Name);
        Assert.Equal(100, info.OwnerId);
    }
    
    [Fact]
    public void GetTerritoryInfo_UnknownId_ThrowsArgumentException()
    {
        // Arrange
        var map = new Map();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            map.GetTerritoryInfo(999));
        
        Assert.Contains("Unknown territory id: 999", exception.Message);
    }

    [Fact]
    public void InitializeTerritories_ValidTerritories_StoresCorrectly()
    {
        // Arrange
        var map = new Map();
        var territories = new[]
        {
            new TerritoryInfo(0, 1,"Grasslands"),
            new TerritoryInfo(1, 2,"Mountains")
        };
        
        // Act
        map.InitializeTerritories(territories);
        
        // Assert
        var info0 = map.GetTerritoryInfo(0);
        Assert.Equal("Grasslands", info0.Name);
        Assert.Equal(1, info0.OwnerId);
        
        var info1 = map.GetTerritoryInfo(1);
        Assert.Equal("Mountains", info1.Name);
        Assert.Equal(2, info1.OwnerId);
    }

    [Fact]
    public void InitializeTerritories_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var map = new Map();
        var territories = new[]
        {
            new TerritoryInfo(999, -1,"Invalid")
        };
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            map.InitializeTerritories(territories));
    }

    #endregion

    #region MapGenerator Tests

    [Fact]
    public void MapGenerator_Generate_InitializesAllTerritories()
    {
        // Arrange
        var generator = new MapGenerator(seed: 12345);
        
        // Act
        var map = generator.Generate();
        
        // Assert - проверяем несколько территорий
        var info0 = map.GetTerritoryInfo(0);
        Assert.Equal(0, info0.Id);
        Assert.Equal("Territory 0", info0.Name);
        Assert.Equal(-1, info0.OwnerId);
        
        var info50 = map.GetTerritoryInfo(50);
        Assert.Equal(50, info50.Id);
        Assert.Equal("Territory 50", info50.Name);
        
        var info86 = map.GetTerritoryInfo(86);
        Assert.Equal(86, info86.Id);
        Assert.Equal("Territory 86", info86.Name);
    }

    [Fact]
    public void MapGenerator_WithSeed_ProducesDeterministicResults()
    {
        // Arrange
        const int seed = 42;
        var generator1 = new MapGenerator(seed);
        var generator2 = new MapGenerator(seed);
        
        // Act
        var map1 = generator1.Generate();
        var map2 = generator2.Generate();
        
        // Assert - проверяем несколько случайных точек
        for (int i = 0; i < 10; i++)
        {
            int x = i * 100;
            int y = i * 100;
            
            Assert.Equal(map1.GetTileType(x, y), map2.GetTileType(x, y));
            Assert.Equal(map1.GetTerritoryId(x, y), map2.GetTerritoryId(x, y));
        }
    }

    [Fact]
    public void MapGenerator_Generate_HasReasonableMountainDistribution()
    {
        // Arrange
        var generator = new MapGenerator(seed: 12345);
        var map = generator.Generate();
        
        // Act - подсчитываем горы в случайной выборке
        int mountainCount = 0;
        int sampleSize = 1000;
        var random = new Random(12345);
        
        for (int i = 0; i < sampleSize; i++)
        {
            int x = random.Next(0, Map.Width);
            int y = random.Next(0, Map.Height);
            
            if (map.GetTileType(x, y) == TileType.Mountain)
                mountainCount++;
        }
        
        double mountainRatio = (double)mountainCount / sampleSize;
        
        Assert.InRange(mountainRatio, 0.20, 0.40);
    }

    #endregion
}