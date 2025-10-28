namespace MapRegions;

public sealed class VoronoiRegionGenerator : IRegionGenerator
{
    private readonly Random _random;

    public VoronoiRegionGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public ushort[] Generate(int width, int height, int regionCount)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));
        if (regionCount <= 0)
            throw new ArgumentException("Region count must be positive", nameof(regionCount));
        if (regionCount > ushort.MaxValue)
            throw new ArgumentException($"Region count cannot exceed {ushort.MaxValue}", nameof(regionCount));

        var regions = new ushort[width * height];
        int tilesPerRegion = (width * height) / regionCount;
        var regionSizes = new int[regionCount];
        
        var centers = GenerateRegionCenters(width, height, regionCount);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ushort nearestRegion = FindNearestRegion(x, y, centers);
                regions[y * width + x] = nearestRegion;
            }
        }

        foreach (var region in regions)
        {
            regionSizes[region]++;
        }

        for (int iteration = 0; iteration < 100; iteration++)
        {
            bool changed = false;

            for (int i = 0; i < regions.Length; i++)
            {
                ushort currentRegion = regions[i];
                
                if (regionSizes[currentRegion] <= tilesPerRegion)
                    continue;

                int x = i % width;
                int y = i / width;

                ushort? smallerNeighbor = FindSmallerNeighbor(x, y, width, height, regions, regionSizes, tilesPerRegion);
                
                if (smallerNeighbor.HasValue)
                {
                    regionSizes[currentRegion]--;
                    regionSizes[smallerNeighbor.Value]++;
                    regions[i] = smallerNeighbor.Value;
                    changed = true;
                }
            }

            if (!changed)
                break;
        }

        return regions;
    }

    private (int x, int y)[] GenerateRegionCenters(int width, int height, int regionCount)
    {
        var centers = new (int x, int y)[regionCount];
        for (ushort i = 0; i < regionCount; i++)
        {
            centers[i] = (_random.Next(width), _random.Next(height));
        }
        return centers;
    }

    private ushort FindNearestRegion(int x, int y, (int x, int y)[] centers)
    {
        ushort nearestRegion = 0;
        double minDistance = double.MaxValue;

        for (ushort i = 0; i < centers.Length; i++)
        {
            var (cx, cy) = centers[i];
            double distance = (x - cx) * (x - cx) + (y - cy) * (y - cy);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestRegion = i;
            }
        }

        return nearestRegion;
    }

    private ushort? FindSmallerNeighbor(int x, int y, int width, int height, ushort[] regions, int[] regionSizes, int targetSize)
    {
        ushort? best = null;
        int minSize = int.MaxValue;

        var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
        
        foreach (var (dx, dy) in directions)
        {
            int nx = x + dx;
            int ny = y + dy;

            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                ushort neighborRegion = regions[ny * width + nx];
                int neighborSize = regionSizes[neighborRegion];

                if (neighborSize < targetSize && neighborSize < minSize)
                {
                    best = neighborRegion;
                    minSize = neighborSize;
                }
            }
        }

        return best;
    }
}

