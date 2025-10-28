using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class UnitDto
{
    public string Id { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
}

