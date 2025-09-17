namespace TileMapEngine.Models;
using System.Drawing;
using Enums;

public class TileLayer
{
    private readonly byte[,] _data;
    public int Width { get; }
    public int Height { get; }
    public TileLayer(int width, int height)
    {
        Width = width;
        Height = height;
        _data = new byte[width, height];
    }
    public SurfaceType GetTile(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException($"Invalid tile position: {x}, {y}");
        }
        
        return _data[x, y] == (byte)SurfaceType.Mountain ? SurfaceType.Mountain : SurfaceType.Plain;
    }
    
    public void SetTile(int x, int y, SurfaceType type)
    {
        if (!IsInBounds(x, y))
        {
            throw new ArgumentException($"Coordinates out of bounds: {x}, {y}");
        }
        _data[x, y] = (byte)type;
    }

    public void FillRegion(Rectangle area, SurfaceType type)
    {
        if (!IsAreaValid(area))
        {
            throw new ArgumentException($"Invalid area: {area}");
        }

        for (int y = area.Top; y < area.Bottom; y++)
        {
            for (int x = area.Left; x < area.Right; x++)
            {
                _data[x, y] = (byte)type;
            }
        }
    }

    public bool CanPlaceObject(int x, int y)
    {
        for (int i = x; i < x + Width; i++)
        for (int j = y; j < y + Height; j++)
            if (GetTile(i, j) != SurfaceType.Plain)
                return false;
        return true;
    }
    
    public bool CanPlaceObjectInArea(Rectangle area)
    {
        if (!IsAreaValid(area))
        {
            throw new ArgumentException($"Invalid area: {area}");
        }
        
        for (int y = area.Top; y < area.Bottom; y++)
        {
            for (int x = area.Left; x < area.Right; x++)
            {
                if (!CanPlaceObject(x, y))
                    return false;
            }
        }
        return true;
    }

    public static TileLayer FromEnumerable(IEnumerable<(int x, int y, SurfaceType type)> tiles, int width, int height)
    {
        var layer = new TileLayer(width, height);
        foreach (var tile in tiles)
        {
            layer.SetTile(tile.x, tile.y, tile.type);
        }
        return layer;
    }

    private int GetIndex(int x, int y) => y * Width + x;
    private bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
    private bool IsAreaValid(Rectangle area) =>
        area.X >= 0 && area.Width <= Width &&
        area.Y >= 0 && area.Height <= Height;
}