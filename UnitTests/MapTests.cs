using MapLib;
using MapLib.Enum;

namespace UnitTests;

public class MapTests
{
    [Fact]
    public void Constructor_InitializesEmptyMap()
    {
        // Act
        var map = new Map();
        
        // Assert
        Assert.Equal(TileType.Plain, map.GetTileType(0, 0));
        Assert.Equal(0, map.GetTerritoryId(0, 0));
    }

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
    [InlineData(0, -1)]
    [InlineData(1000, 0)]
    [InlineData(0, 1000)]
    [InlineData(1000, 1000)]
    [InlineData(-1, -1)]
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

    #region GetTileType Tests

    [Fact]
    public void GetTileType_ValidCoordinates_ReturnsTileType()
    {
        // Arrange
        var map = new Map();
        map.SetTile(100, 200, TileType.Mountain, 10);
        
        // Act
        var tileType = map.GetTileType(100, 200);
        
        // Assert
        Assert.Equal(TileType.Mountain, tileType);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(1000, 0)]
    [InlineData(0, 1000)]
    public void GetTileType_InvalidCoordinates_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        // Arrange
        var map = new Map();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetTileType(x, y));
    }

    #endregion

    #region GetTerritoryId Tests

    [Fact]
    public void GetTerritoryId_ValidCoordinates_ReturnsTerritoryId()
    {
        // Arrange
        var map = new Map();
        map.SetTile(50, 75, TileType.Mountain, 42);
        
        // Act
        var territoryId = map.GetTerritoryId(50, 75);
        
        // Assert
        Assert.Equal(42, territoryId);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(1000, 0)]
    [InlineData(0, 1000)]
    public void GetTerritoryId_InvalidCoordinates_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        // Arrange
        var map = new Map();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetTerritoryId(x, y));
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
    [InlineData(0, -1, TileType.Plain, 1)]
    [InlineData(1000, 0, TileType.Plain, 1)]
    [InlineData(0, 1000, TileType.Plain, 1)]
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
    [InlineData(-100)]
    [InlineData(87)]
    [InlineData(100)]
    public void SetTile_InvalidTerritoryId_ThrowsArgumentOutOfRangeException(int territoryId)
    {
        // Arrange
        var map = new Map();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            map.SetTile(0, 0, TileType.Plain, territoryId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(86)]
    public void SetTile_ValidTerritoryIdRange_SetsSuccessfully(int territoryId)
    {
        // Arrange
        var map = new Map();
        
        // Act
        map.SetTile(0, 0, TileType.Plain, territoryId);
        
        // Assert
        Assert.Equal(territoryId, map.GetTerritoryId(0, 0));
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

    [Fact]
    public void SetTile_MultipleLocations_DoesNotAffectOtherTiles()
    {
        // Arrange
        var map = new Map();
        map.SetTile(0, 0, TileType.Plain, 1);
        map.SetTile(10, 10, TileType.Mountain, 2);
        
        // Act
        map.SetTile(5, 5, TileType.Mountain, 3);
        
        // Assert
        Assert.Equal(TileType.Plain, map.GetTileType(0, 0));
        Assert.Equal(1, map.GetTerritoryId(0, 0));
        Assert.Equal(TileType.Mountain, map.GetTileType(10, 10));
        Assert.Equal(2, map.GetTerritoryId(10, 10));
        Assert.Equal(TileType.Mountain, map.GetTileType(5, 5));
        Assert.Equal(3, map.GetTerritoryId(5, 5));
    }

    #endregion

    #region Packing/Unpacking Tests

    [Fact]
    public void PackedTile_Format_CorrectBitLayout()
    {
        // Arrange
        var map = new Map();
        const byte tileTypeValue = 18;
        const int territoryIdValue = 52;
        
        map.SetTile(0, 0, (TileType)tileTypeValue, territoryIdValue);
        
        // Act
        long packedTile = map.GetTile(0, 0);
        
        // Assert
        const long lowerByteMask = 0xFF;           // Маска для младшего байта (биты 0-7)
        const long upperBitsMask = 0xFFFFFFFF;     // Маска для следующих 32 бит
        const int territoryIdShift = 8;             // Сдвиг для territoryId
        
        long extractedTileType = packedTile & lowerByteMask;
        long extractedTerritoryId = (packedTile >> territoryIdShift) & upperBitsMask;
        
        Assert.Equal(tileTypeValue, extractedTileType);
        Assert.Equal(territoryIdValue, extractedTerritoryId);
    }

    [Fact]
    public void PackedTile_Format_VerifyPackingAlgorithm()
    {
        // Arrange
        var map = new Map();
        const byte tileTypeValue = 127;
        const int territoryIdValue = 42;
        
        map.SetTile(0, 0, (TileType)tileTypeValue, territoryIdValue);
        
        // Act
        long packedTile = map.GetTile(0, 0);
        
        // Assert - проверяем формулу упаковки: type | (territoryId << 8)
        const int territoryIdShift = 8;
        long expectedPacked = ((long)tileTypeValue) | ((long)territoryIdValue << territoryIdShift);
        
        Assert.Equal(expectedPacked, packedTile);
        
        TileType unpackedType = map.GetTileTypeFromPacked(packedTile);
        int unpackedTerritoryId = map.GetTerritoryIdFromPacked(packedTile);
        
        Assert.Equal((TileType)tileTypeValue, unpackedType);
        Assert.Equal(territoryIdValue, unpackedTerritoryId);
    }

    [Fact]
    public void PackedTile_MaximumValues_PacksAndUnpacksCorrectly()
    {
        // Arrange
        var map = new Map();
        const byte maxTileType = 255;
        const int maxTerritoryId = 86;
        
        map.SetTile(0, 0, (TileType)maxTileType, maxTerritoryId);
        
        // Act
        long packedTile = map.GetTile(0, 0);
        
        // Assert
        TileType unpackedType = map.GetTileTypeFromPacked(packedTile);
        int unpackedTerritoryId = map.GetTerritoryIdFromPacked(packedTile);
        
        Assert.Equal((TileType)maxTileType, unpackedType);
        Assert.Equal(maxTerritoryId, unpackedTerritoryId);
    }

    #endregion

    #region GetTerritoryInfo Tests
    
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

    #endregion
}