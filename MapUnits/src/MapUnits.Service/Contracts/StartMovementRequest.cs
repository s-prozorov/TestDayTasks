using MessagePack;

namespace MapUnits.Service.Contracts;

[MessagePackObject]
public class StartMovementRequest
{
    [Key(0)]
    public string UnitId { get; set; } = string.Empty;
    
    [Key(1)]
    public float TargetX { get; set; }
    
    [Key(2)]
    public float TargetY { get; set; }
}


