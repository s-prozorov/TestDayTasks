namespace TileMapEngine.Models;
using System.Collections;
using System.Drawing;
using Enums;

public struct TileLayer(int width, int height)
{
    private static readonly bool[] CanPlaceObject = [true, false];

    private static bool GetCanPlaceObject(SurfaceType type) => CanPlaceObject[(byte)type];

    private readonly BitArray _data = new BitArray(width * height);
    public int Width { get; } = width;
    public int Height { get; } = height;

    public SurfaceType GetTile(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException($"Invalid tile position: {x}, {y}");
        }
        
        return _data[GetIndex(x, y)] ? SurfaceType.Mountain : SurfaceType.Plain;
    }
    
    public void SetTile(int x, int y, SurfaceType type)
    {
        if (!IsInBounds(x, y))
        {
            throw new ArgumentException($"Coordinates out of bounds: {x}, {y}");
        }
        _data[GetIndex(x, y)] = type == SurfaceType.Mountain;
    }

    public void FillRegion(Rectangle area, SurfaceType type)
    {
        if (!IsAreaValid(area))
        {
            throw new ArgumentException($"Invalid area: {area}");
        }

        var value = type == SurfaceType.Mountain;
        for (int y = area.Top; y < area.Bottom; y++)
        {
            for (int x = area.Left; x < area.Right; x++)
            {
                _data[GetIndex(x, y)] = value;
            }
        }
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
                if (!GetCanPlaceObject(GetTile(x, y)))
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