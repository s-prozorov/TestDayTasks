namespace MapSystem.Core.SurfaceLayer;

/// <summary>
///     Интерфейс для работы со слоем поверхности карты.
/// </summary>
public interface ISurfaceLayer
{
    int Width { get; }
    int Height { get; }

    TileType GetTileType(int x, int y);
    void SetTileType(int x, int y, TileType type);
    void FillArea(int x, int y, int width, int height, TileType type);
    bool CanPlaceInArea(int x, int y, int width, int height);
}
