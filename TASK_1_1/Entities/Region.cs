namespace TASK_1_1.Entities;

public class Region
{
    public ushort Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TilePosition[] Tiles { get; init; } = Array.Empty<TilePosition>();
}

public struct TilePosition
{
    public int X;
    public int Y;
}
