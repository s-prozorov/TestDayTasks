using TASK_1_1.Infrastructure;

namespace TASK_1_1.Layers;
public class SurfaceLayer : ISurfaceLayer<TileEnum>
{
    private TileEnum[] tiles;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public SurfaceLayer(int width, int height)
    {
        Width = width;
        Height = height;
        tiles = new TileEnum[width * height];
    }

    private int Index(int x, int y) => y * Width + x;

    public TileEnum GetTileType(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException();
        return tiles[Index(x, y)];
    }

    public void SetTileType(int x, int y, TileEnum type)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException();
        tiles[Index(x, y)] = type;
    }

    public void Fill(TileEnum type)
    {
        for (int i = 0; i < tiles.Length; i++)
            tiles[i] = type;
    }

    public void FillArea(int startX, int startY, int endX, int endY, TileEnum type)
    {
        int minX = Math.Max(0, startX);
        int maxX = Math.Min(Width - 1, endX);
        int minY = Math.Max(0, startY);
        int maxY = Math.Min(Height - 1, endY);

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                tiles[Index(x, y)] = type;
    }

    public bool CanPlaceObject(int startX, int startY, int endX, int endY)
    {
        for (int y = startY; y <= endY; y++)
            for (int x = startX; x <= endX; x++)
                if (GetTileType(x, y) == TileEnum.Mountain)
                    return false;
        return true;
    }

    public void Resize(int newWidth, int newHeight, TileEnum defaultType)
    {
        var newTiles = new TileEnum[newWidth * newHeight];

        for (int y = 0; y < Math.Min(Height, newHeight); y++)
            for (int x = 0; x < Math.Min(Width, newWidth); x++)
                newTiles[y * newWidth + x] = tiles[Index(x, y)];

        for (int i = 0; i < newTiles.Length; i++)
            if (newTiles[i] == 0) newTiles[i] = defaultType;

        tiles = newTiles;
        Width = newWidth;
        Height = newHeight;
    }
}
