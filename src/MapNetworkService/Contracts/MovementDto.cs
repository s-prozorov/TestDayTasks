using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class MovementDto
{
    public string UnitId { get; set; } = string.Empty;
    public float StartX { get; set; }
    public float StartY { get; set; }
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public float Speed { get; set; }
}

