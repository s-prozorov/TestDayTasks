using MapLib.Core.Models;
using MapLib.Core.Models.TerritoryModel;
using MapLib.Core.Models.TileModel;

namespace MapLib.Core.Services;

public static class TerritoryGenService
{
    public static void GenerateTerritories(int count, Dictionary<int, Territory> territories, Tile[,] tiles)
    {
        var random = new Random();

        var centers = new List<(int x, int y)>();
        for (int i = 0; i < count; i++)
        {
            centers.Add((random.Next(0, 1000), random.Next(0, 1000)));
        }

        for (int x = 0; x < 1000; x++)
        {
            for (int y = 0; y < 1000; y++)
            {
                int closestTerritoryId = 0;
                double minDistance = double.MaxValue;

                for (int i = 0; i < centers.Count; i++)
                {
                    double distance = Math.Sqrt(Math.Pow(x - centers[i].x, 2) + Math.Pow(y - centers[i].y, 2));
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestTerritoryId = i;
                    }
                }

                tiles[x, y] = new Tile()
                {
                    Type = random.Next(0, 100) < 70 ? TileType.Plane : TileType.Mountain,
                    TerritoryId = closestTerritoryId,
                    X = x,
                    Y = y
                };
            }
        }

        for (int i = 0; i < count; i++)
        {
            var territoryTiles = new List<Tile>();
            int minX = 1000, minY = 1000, maxX = 0, maxY = 0;

            for (int x = 0; x < 1000; x++)
            {
                for (int y = 0; y < 1000; y++)
                {
                    if (tiles[x, y].TerritoryId == i)
                    {
                        territoryTiles.Add(tiles[x, y]);
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }

            territories[i] = new Territory(i, minX, minY, maxX - minX + 1, maxY - minY + 1)
            {
                Tiles = territoryTiles.ToArray()
            };
        }
    }
}

