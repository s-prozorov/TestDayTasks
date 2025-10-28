using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class StartMovementRequest
{
    public string UnitId { get; set; } = string.Empty;
    public float TargetX { get; set; }
    public float TargetY { get; set; }
}

