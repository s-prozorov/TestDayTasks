using MemoryPack;

namespace MapSystem.Contracts;

[MemoryPackable]
public partial class RegionData
{
    public ushort Id { get; set; }
    public string Name { get; set; } = string.Empty;
}