

using MapSystem.Core.Common;

namespace MapSystem.Core.RegionLayer;

public sealed class RegionsLayer : IRegionLayer
{
    private readonly ushort[] _regionIds;
    private readonly Dictionary<ushort, Region> _regions;

    private readonly int _regionWidth;
    private readonly int _regionHeight;
    private readonly int _regionsPerSide;
    private RegionsLayer(
        int width,
        int height,
        ushort[] regionIds,
        Dictionary<ushort, Region> regions,
        int regionWidth,
        int regionHeight, 
        int regionsPerSide)
    {
        Width = width;
        Height = height;
        _regionIds = regionIds;
        _regions = regions;
        _regionWidth = regionWidth;
        _regionHeight = regionHeight;
        _regionsPerSide = regionsPerSide;
    }

    public int Width { get; }
    public int Height { get; }

    public ushort GetRegionId(int x, int y)
    {
        ValidateHelpers.ValidatePoint(x, y, Width, Height);
        var index = ValidateHelpers.GetIndex(x, y, Width);
        return _regionIds[index];
    }

    public Region GetRegion(ushort id)
    {
        if (!_regions.TryGetValue(id, out var region))
            throw new ArgumentException($"Регион с ID {id} не найден", nameof(id));

        return region;
    }

    public bool IsInRegion(int x, int y, ushort regionId)
    {
        ValidateHelpers.ValidatePoint(x, y, Width, Height);

        if (!_regions.ContainsKey(regionId))
            return false;

        var actualRegionId = GetRegionId(x, y);
        return actualRegionId == regionId;
    }

    public IReadOnlyCollection<Region> GetRegionsInArea(int x, int y, int width, int height)
    {
        ValidateHelpers.ValidateArea(x, y, width, height);
        ValidateHelpers.ValidateAreaBounds(x, y, width, height, Width, Height);

        
        var startGridX = x / _regionWidth;
        var startGridY = y / _regionHeight;
        var endGridX = Math.Min((x + width - 1) / _regionWidth, _regionsPerSide - 1);
        var endGridY = Math.Min((y + height - 1) / _regionHeight, _regionsPerSide - 1);

        var regions = new List<Region>();
    
        for (var gridY = startGridY; gridY <= endGridY; gridY++)
        for (var gridX = startGridX; gridX <= endGridX; gridX++)
        {
            var regionId = (ushort)(gridY * _regionsPerSide + gridX);
            if (_regions.TryGetValue(regionId, out var region))
            {
                regions.Add(region);
            }
        }

        return regions;
    }

    public static RegionsLayer Create(
        int width,
        int height,
        int targetRegionCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width, nameof(width));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height, nameof(height));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetRegionCount, nameof(targetRegionCount));

        var regionsPerSide = ValidateHelpers.CalculateRegionsPerSide(targetRegionCount);

        var regionIds = new ushort[width * height];
        var regions = new Dictionary<ushort, Region>();

        var regionWidth = width / regionsPerSide;
        var regionHeight = height / regionsPerSide;

        ushort regionId = 0;

        for (var gridY = 0; gridY < regionsPerSide; gridY++)
        for (var gridX = 0; gridX < regionsPerSide; gridX++)
        {
            var regionName = $"Region [{gridX},{gridY}]";
            var region = new Region(regionId, regionName);
            regions[regionId] = region;

            var startX = gridX * regionWidth;
            var startY = gridY * regionHeight;

            var endX = (gridX == regionsPerSide - 1) ? width : startX + regionWidth;
            var endY = (gridY == regionsPerSide - 1) ? height : startY + regionHeight;

            for (var tileY = startY; tileY < endY; tileY++)
            for (var tileX = startX; tileX < endX; tileX++)
            {
                var index = ValidateHelpers.GetIndex(tileX, tileY, width);
                regionIds[index] = regionId;
            }

            regionId++;
        }

        return new RegionsLayer(
            width, 
            height, 
            regionIds, 
            regions,
            regionWidth,      
            regionHeight,     
            regionsPerSide);  
    }
}