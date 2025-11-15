namespace MapSystem.Core.ObjectLayer;

public sealed class MapObject
{
    internal MapObject(uint id, int x, int y, int width, int height)
    {
        Id = id;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public uint Id { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
}