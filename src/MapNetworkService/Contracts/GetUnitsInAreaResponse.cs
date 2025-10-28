using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class GetUnitsInAreaResponse
{
    public UnitDto[] Units { get; set; } = Array.Empty<UnitDto>();
    public MovementDto[] Movements { get; set; } = Array.Empty<MovementDto>();
}

