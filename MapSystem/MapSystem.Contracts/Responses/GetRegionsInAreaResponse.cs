using MemoryPack;

namespace MapSystem.Contracts;

[MemoryPackable]
public partial class GetRegionsInAreaResponse
{
    public RegionData[] Regions { get; set; } = Array.Empty<RegionData>();
}