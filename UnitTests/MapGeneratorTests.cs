using MapLib;
using MapLib.Enum;

namespace UnitTests;

public class MapGeneratorTests
{
    [Fact]
    public void Generate_InitializesAllTerritories()
    {
        // Arrange
        var generator = new MapGenerator(seed: 42);
        
        // Act
        var map = generator.Generate();
        
        // Assert
        for (int i = 0; i < Map.MaxTerritories; i++)
        {
            var territory = map.GetTerritoryInfo(i);
            Assert.Equal(i, territory.Id);
            Assert.Equal($"Territory {i}", territory.Name);
            Assert.Equal(-1, territory.OwnerId);
        }
    }

    [Fact]
    public void Generate_AssignsValidTerritoryToEachTile()
    {
        // Arrange
        var generator = new MapGenerator(seed: 42);
        
        // Act
        var map = generator.Generate();
        
        // Assert - проверяем случайные тайлы
        var random = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            int x = random.Next(0, Map.Width);
            int y = random.Next(0, Map.Height);
            
            int territoryId = map.GetTerritoryId(x, y);
            Assert.InRange(territoryId, 0, Map.MaxTerritories - 1);
        }
    }

    [Fact]
    public void Generate_AssignsValidTileTypes()
    {
        // Arrange
        var generator = new MapGenerator(seed: 42);
        
        // Act
        var map = generator.Generate();
        
        // Assert - проверяем случайные тайлы
        var random = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            int x = random.Next(0, Map.Width);
            int y = random.Next(0, Map.Height);
            
            var tileType = map.GetTileType(x, y);
            Assert.True(tileType == TileType.Plain || tileType == TileType.Mountain);
        }
    }

    [Fact]
    public void Generate_WithSeed_ProducesDeterministicResults()
    {
        // Arrange
        const int seed = 123;
        var generator1 = new MapGenerator(seed);
        var generator2 = new MapGenerator(seed);
        
        // Act
        var map1 = generator1.Generate();
        var map2 = generator2.Generate();
        
        // Assert - проверяем несколько случайных точек
        var testPoints = new[] { (0, 0), (100, 100), (500, 500), (999, 999) };
        
        foreach (var (x, y) in testPoints)
        {
            Assert.Equal(map1.GetTileType(x, y), map2.GetTileType(x, y));
            Assert.Equal(map1.GetTerritoryId(x, y), map2.GetTerritoryId(x, y));
        }
    }

    [Fact]
    public void Generate_WithoutSeed_ProducesRandomResults()
    {
        // Arrange
        var generator1 = new MapGenerator();
        var generator2 = new MapGenerator();
        
        // Act
        var map1 = generator1.Generate();
        var map2 = generator2.Generate();
        
        // Assert - хотя бы один тайл должен отличаться
        bool foundDifference = false;
        
        for (int i = 0; i < 100 && !foundDifference; i++)
        {
            int x = i * 10;
            int y = i * 10;
            
            if (map1.GetTileType(x, y) != map2.GetTileType(x, y) ||
                map1.GetTerritoryId(x, y) != map2.GetTerritoryId(x, y))
            {
                foundDifference = true;
            }
        }
        
        Assert.True(foundDifference, "Maps should be different without seed");
    }

    [Fact]
    public void Generate_MountainDistribution_IsApproximatelyThirtyPercent()
    {
        // Arrange
        var generator = new MapGenerator(seed: 42);
        var map = generator.Generate();
        
        // Act - подсчитываем горы в выборке
        int mountainCount = 0;
        int sampleSize = 1000;
        var random = new Random(42);
        
        for (int i = 0; i < sampleSize; i++)
        {
            int x = random.Next(0, Map.Width);
            int y = random.Next(0, Map.Height);
            
            if (map.GetTileType(x, y) == TileType.Mountain)
                mountainCount++;
        }
        
        double mountainRatio = (double)mountainCount / sampleSize;
        
        // Assert - ожидаем 30% +- 10%
        Assert.InRange(mountainRatio, 0.20, 0.40);
    }

    [Fact]
    public void Generate_NeighboringTiles_CanHaveDifferentTerritories()
    {
        // Arrange
        var generator = new MapGenerator(seed: 42);
        
        // Act
        var map = generator.Generate();
        
        // Assert - ищем границу между территориями
        bool foundBorder = false;
        
        for (int x = 0; x < Map.Width - 1 && !foundBorder; x++)
        {
            for (int y = 0; y < Map.Height - 1 && !foundBorder; y++)
            {
                int territory1 = map.GetTerritoryId(x, y);
                int territory2 = map.GetTerritoryId(x + 1, y);
                int territory3 = map.GetTerritoryId(x, y + 1);
                
                if (territory1 != territory2 || territory1 != territory3)
                {
                    foundBorder = true;
                }
            }
        }
        
        Assert.True(foundBorder, "Map should have territory borders");
    }
}