using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class RegionDto
{
    public ushort Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

