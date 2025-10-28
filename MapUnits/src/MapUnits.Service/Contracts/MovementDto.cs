using MessagePack;

namespace MapUnits.Service.Contracts;

[MessagePackObject]
public class MovementDto
{
    [Key(0)]
    public string UnitId { get; set; } = string.Empty;
    
    [Key(1)]
    public float StartX { get; set; }
    
    [Key(2)]
    public float StartY { get; set; }
    
    [Key(3)]
    public float TargetX { get; set; }
    
    [Key(4)]
    public float TargetY { get; set; }
    
    [Key(5)]
    public float Speed { get; set; }
}


