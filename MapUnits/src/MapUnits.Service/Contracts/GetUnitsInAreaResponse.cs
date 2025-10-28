using MessagePack;

namespace MapUnits.Service.Contracts;

[MessagePackObject]
public class GetUnitsInAreaResponse
{
    [Key(0)]
    public UnitDto[] Units { get; set; } = Array.Empty<UnitDto>();
    
    [Key(1)]
    public MovementDto[] Movements { get; set; } = Array.Empty<MovementDto>();
}


