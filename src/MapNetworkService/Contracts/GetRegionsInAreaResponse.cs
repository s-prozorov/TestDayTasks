using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class GetRegionsInAreaResponse
{
    public RegionDto[] Regions { get; set; } = Array.Empty<RegionDto>();
}

