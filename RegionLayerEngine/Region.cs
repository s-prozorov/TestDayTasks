namespace RegionLayerEngine;

public class Region
{
    public ushort Id { get; }
    public string Name { get; }
    public List<(int X, int Y)> Tiles { get; } // Для отладки и расширения
    
    // Расширяемая часть
    public Dictionary<string, object> Properties { get; }
    
    public Region(ushort id, string name)
    {
        Id = id;
        Name = name;
        Properties = new Dictionary<string, object>();
    }
}