namespace TASK_1_1;
public class MapObject
{
    public string Id { get; init; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public MapObject(string id, int x, int y, int width, int height)
    {
        Id = id;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Contains(int x, int y) =>
        x >= X && x < X + Width && y >= Y && y < Y + Height;

    public bool Intersects(int startX, int startY, int endX, int endY) =>
        !(X + Width <= startX || X >= endX || Y + Height <= startY || Y >= endY);
}

