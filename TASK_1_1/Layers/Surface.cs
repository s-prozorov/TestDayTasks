using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK_1_1.Infrastructure;

namespace TASK_1_1.Layers;
public class Surface : ISurfaceLayer<TileEnum>
{
    private TileEnum[,] tiles;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Surface(int width, int height)
    {
        Width = width;
        Height = height;
        tiles = new TileEnum[width, height];
    }

    public TileEnum GetTileType(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException($"Координаты ({x},{y}) находятся за пределами карты.");
        return tiles[x, y];
    }

    public void SetTileType(int x, int y, TileEnum type)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException($"Координаты ({x},{y}) находятся за пределами карты.");
        tiles[x, y] = type;
    }

    public void ReplaceTiles(TileEnum[,] newTiles)
    {
        int newWidth = newTiles.GetLength(0);
        int newHeight = newTiles.GetLength(1);

        tiles = newTiles;
        Width = newWidth;
        Height = newHeight;
    }

    public void FillArea(int startX, int startY, int endX, int endY, TileEnum type)
    {
        int minX = Math.Max(0, startX);
        int maxX = Math.Min(Width - 1, endX);
        int minY = Math.Max(0, startY);
        int maxY = Math.Min(Height - 1, endY);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                tiles[x, y] = type;
    }

    public bool CanPlaceObject(int startX, int startY, int endX, int endY)
    {
        int minX = Math.Max(0, startX);
        int maxX = Math.Min(Width - 1, endX);
        int minY = Math.Max(0, startY);
        int maxY = Math.Min(Height - 1, endY);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (tiles[x, y] == TileEnum.Mountain)
                    return false;
            }
        }
        return true;
    }

    public void Fill(TileEnum type)
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                tiles[x, y] = type;
    }

    public void Resize(int newWidth, int newHeight, TileEnum defaultType = TileEnum.Plain)
    {
        var newTiles = new TileEnum[newWidth, newHeight];

        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                if (x < Width && y < Height)
                {
                    newTiles[x, y] = tiles[x, y];
                }
                else
                {
                    newTiles[x, y] = defaultType;
                }
            }
        }

        tiles = newTiles;
        Width = newWidth;
        Height = newHeight;
    }

    public static Surface FromArray(TileEnum[,] source) { 
        var surface = new Surface(source.GetLength(0), source.GetLength(1)); 
        surface.ReplaceTiles(source); 
        return surface; 
    }
}
