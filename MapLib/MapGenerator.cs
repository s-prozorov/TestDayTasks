using MapLib.Enum;
using MapLib.Models;

namespace MapLib;

public class MapGenerator
{
    private const double MOUNTAIN_PROBABILITY = 0.3;
    
    private readonly Random _random;

    public MapGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }
    
    /// <summary>
    /// Генерирует карту с использованием алгоритма Вороного для территорий.
    /// </summary>
    public Map Generate()
    {
        var map = new Map();
        
        var territoryCenters = CreateTerritoryCenters();
        
        InitializeTerritories(map, territoryCenters);
        
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                int territoryId = FindNearestTerritory(x, y, territoryCenters);
                
                TileType type = _random.NextDouble() < MOUNTAIN_PROBABILITY 
                    ? TileType.Mountain 
                    : TileType.Plain;

                map.SetTile(x, y, type, territoryId);
            }
        }
        
        return map;
    }
    
    private List<(int x, int y)> CreateTerritoryCenters()
    {
        var centers = new List<(int x, int y)>(Map.MaxTerritories);

        int gridSize = (int)Math.Ceiling(Math.Sqrt(Map.MaxTerritories));
        int cellWidth = Map.Width / gridSize;
        int cellHeight = Map.Height / gridSize;

        for (int i = 0; i < Map.MaxTerritories; i++)
        {
            int gridX = i % gridSize;
            int gridY = i / gridSize;

            int centerX = gridX * cellWidth + cellWidth / 2;
            int centerY = gridY * cellHeight + cellHeight / 2;
            
            int offsetX = _random.Next(-cellWidth / 4, cellWidth / 4);
            int offsetY = _random.Next(-cellHeight / 4, cellHeight / 4);

            int x = Math.Clamp(centerX + offsetX, 0, Map.Width - 1);
            int y = Math.Clamp(centerY + offsetY, 0, Map.Height - 1);

            centers.Add((x, y));
        }

        return centers;
    }
    
    /// <summary>
    /// Инициализирует базовую информацию о территориях на карте.
    /// </summary>
    private void InitializeTerritories(Map map, List<(int x, int y)> centers)
    {
        var territories = new List<TerritoryInfo>(Map.MaxTerritories);
        
        for (int i = 0; i < Map.MaxTerritories; i++)
        {
            territories.Add(new TerritoryInfo(
                Id: i,
                Name: $"Territory {i}",
                OwnerId: -1 
            ));
        }
        
        map.InitializeTerritories(territories);
    }
    
    /// <summary>
    /// Находит ближайшую территорию для заданной точки
    /// </summary>
    private int FindNearestTerritory(int x, int y, List<(int x, int y)> centers)
    {
        int nearestId = 0;
        double minDistanceSquared = double.MaxValue;

        for (int i = 0; i < centers.Count; i++)
        {
            int dx = x - centers[i].x;
            int dy = y - centers[i].y;
            double distanceSquared = dx * dx + dy * dy;

            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                nearestId = i;
            }
        }

        return nearestId;
    }
}