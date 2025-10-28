using MessagePack;

namespace MapUnits.Service.Contracts;

[MessagePackObject]
public class UnitDto
{
    [Key(0)]
    public string Id { get; set; } = string.Empty;
    
    [Key(1)]
    public float X { get; set; }
    
    [Key(2)]
    public float Y { get; set; }
}


