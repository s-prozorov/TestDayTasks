using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class GetObjectsInAreaResponse
{
    public MapObjectDto[] Objects { get; set; } = Array.Empty<MapObjectDto>();
}

