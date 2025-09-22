using TASK_1_1.Infrastructure;

namespace TASK_1_1;
public class Map
{
    private readonly Dictionary<string, ISurfaceLayer<TileEnum>> surfaceLayers = new();
    private readonly Dictionary<string, IObjectLayer> objectLayers = new();

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void AddSurfaceLayer(string name, ISurfaceLayer<TileEnum> layer)
    {
        if (layer.Width != Width || layer.Height != Height)
            throw new ArgumentException("Размер поверхности должен совпадать с размером карты.");

        surfaceLayers[name] = layer;
    }

    public ISurfaceLayer<TileEnum> GetSurfaceLayer(string name)
    {
        if (!surfaceLayers.ContainsKey(name))
            throw new KeyNotFoundException($"Поверхность '{name}' не наидена.");

        return surfaceLayers[name];
    }

    public bool CanPlaceObject(string layerName, int startX, int startY, int endX, int endY)
    {
        var layer = GetSurfaceLayer(layerName);
        return layer.CanPlaceObject(startX, startY, endX, endY);
    }

    public void Resize(int newWidth, int newHeight, TileEnum defaultType = TileEnum.Plain)
    {
        Width = newWidth;
        Height = newHeight;

        foreach (var layer in surfaceLayers.Values)
        {
            layer.Resize(newWidth, newHeight, defaultType);
        }
    }

    public void AddObjectLayer(string name, IObjectLayer layer)
    {
        if (layer == null) throw new ArgumentNullException(nameof(layer));
        objectLayers[name] = layer;
    }

    public IObjectLayer GetObjectLayer(string name)
    {
        if (!objectLayers.ContainsKey(name))
            throw new KeyNotFoundException($"Слой Объекта '{name}' не наиден.");

        return objectLayers[name];
    }
}
