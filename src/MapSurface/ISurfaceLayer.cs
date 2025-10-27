namespace MapSurface;

public interface ISurfaceLayer
{
    int Width { get; }
    int Height { get; }
    
    TileType GetTileType(int x, int y);
    void SetTileType(int x, int y, TileType tileType);
    void FillArea(int x, int y, int width, int height, TileType tileType);
    bool CanPlaceObjectInArea(int x, int y, int width, int height);
}

