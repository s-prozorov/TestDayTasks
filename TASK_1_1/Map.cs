using TASK_1_1.Entities;
using TASK_1_1.Infrastructure;
using TASK_1_1.Layers;

namespace TASK_1_1;
public class Map
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public SurfaceLayer Surface { get; private set; }
    public IObjectLayer Objects { get; private set; }
    public IRegionLayer Regions { get; private set; }

    public Map(int width, int height, SurfaceLayer surface, IObjectLayer objects, IRegionLayer regions)
    {
        Width = width;
        Height = height;
        Surface = surface ?? throw new ArgumentNullException(nameof(surface));
        Objects = objects ?? throw new ArgumentNullException(nameof(objects));
        Regions = regions ?? throw new ArgumentNullException(nameof(regions));
        if (surface.Width != width || surface.Height != height)
            throw new ArgumentException("Surface size must match map size.");
    }

    public void Resize(int newWidth, int newHeight, TileEnum defaultType = TileEnum.Plain)
    {
        Width = newWidth;
        Height = newHeight;
        Surface.Resize(newWidth, newHeight, defaultType);
        Regions.Resize(newWidth, newHeight);
    }

    public MapObject? GetObjectById(ulong id) => Objects.GetObjectById(id);
    public MapObject? GetObjectByCoordinate(int x, int y) => Objects.GetObjectByCoordinate(x, y);
    public IEnumerable<MapObject> GetObjectsInArea(int startX, int startY, int endX, int endY) =>
        Objects.GetObjectsInArea(startX, startY, endX, endY);

    public bool CanPlaceObjectOnSurface(int startX, int startY, int endX, int endY) =>
        Surface.CanPlaceObject(startX, startY, endX, endY);

    public ushort GetRegionId(int x, int y) => Regions.GetRegionId(x, y);
    public Region GetRegionById(ushort regionId) => Regions.GetRegionById(regionId);
    public bool IsTileInRegion(int x, int y, ushort regionId) => Regions.IsTileInRegion(x, y, regionId);
    public IEnumerable<Region> GetRegionsInArea(int startX, int startY, int endX, int endY) =>
        Regions.GetRegionsInArea(startX, startY, endX, endY);
}
