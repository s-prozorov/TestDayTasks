using MapLib.Enum;

namespace MapLib;

public class MapGenerator
{
    private const double MOINTAIN_PROBABILITY = 0.3;
    
    private Random _random;

    public MapGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }
    
    public Map Generate()
    {
        var map = new Map();
        
        var territoryCenters = CreateTerritoryCenters();
        
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                int territoryId = FindNearestTerritory(x, y, territoryCenters);
                
                TileType type = _random.NextDouble() < MOINTAIN_PROBABILITY 
                    ? TileType.Mountain 
                    : TileType.Plain;

                map.SetTile(x, y, type, territoryId);
            }
        }
        
        return map;
    }
    
    private List<(int x, int y)> CreateTerritoryCenters()
    {
        var centers = new List<(int x, int y)>();

        // Размещаем территории в сетке с небольшим случайным смещением
        int gridSize = (int)Math.Ceiling(Math.Sqrt(Map.MaxTerritories));
        int cellWidth = Map.Width / gridSize;
        int cellHeight = Map.Height / gridSize;

        for (int i = 0; i < Map.MaxTerritories; i++)
        {
            int gridX = i % gridSize;
            int gridY = i / gridSize;

            int centerX = gridX * cellWidth + cellWidth / 2;
            int centerY = gridY * cellHeight + cellHeight / 2;

            // Добавляем случайное смещение
            int offsetX = _random.Next(-cellWidth / 4, cellWidth / 4);
            int offsetY = _random.Next(-cellHeight / 4, cellHeight / 4);

            int x = Math.Clamp(centerX + offsetX, 0, Map.Width - 1);
            int y = Math.Clamp(centerY + offsetY, 0, Map.Height - 1);

            centers.Add((x, y));
        }

        return centers;
    }
    
    private int FindNearestTerritory(int x, int y, List<(int x, int y)> centers)
    {
        int nearestId = 0;
        double minDistance = double.MaxValue;

        for (int i = 0; i < centers.Count; i++)
        {
            int dx = x - centers[i].x;
            int dy = y - centers[i].y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestId = i;
            }
        }

        return nearestId;
    }
}