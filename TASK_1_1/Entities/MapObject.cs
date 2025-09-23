namespace TASK_1_1.Entities;
public class MapObject
{
    public ulong Id { get; init; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public bool Contains(int x, int y) => x >= X && x < X + Width && y >= Y && y < Y + Height;
    public bool Intersects(int startX, int startY, int endX, int endY) =>
        !(X + Width <= startX || X >= endX || Y + Height <= startY || Y >= endY);

    public (double longitude, double latitude) ToGeoCoordinates() => (X, Y);
}

