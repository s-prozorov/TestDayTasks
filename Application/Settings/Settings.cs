namespace Application.Settings;

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost";
    public string StreamName { get; set; } = "objectEvents";
}

public class MapSettings
{
    public int Width { get; set; } = 1000;
    public int Height { get; set; } = 1000;
}

public class RegionSettings
{
    public int Size { get; set; } = 100;
    public string NamePattern { get; set; } = "Region {0}";
}