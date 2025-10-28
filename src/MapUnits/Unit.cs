namespace MapUnits.Core;

public class Unit(string id, float x, float y, float baseSpeed = 1.0f)
{
    public string Id { get; } = id;
    public float X { get; set; } = x;
    public float Y { get; set; } = y;
    public float BaseSpeed { get; } = baseSpeed;

    public (int, int) GetGridCell()
    {
        return ((int)X, (int)Y);
    }
}

