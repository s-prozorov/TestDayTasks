using MemoryPack;
using MessagePack;

namespace MapService.DTO.Requests;

[MessagePackObject]
public partial class GetTerritoriesInAreaRequest
{
    [Key(0)]
    public int X1 { get; set; }
    
    [Key(1)]
    public int X2 { get; set; }
    
    [Key(2)]
    public int Y1 { get; set; }
    
    [Key(3)]
    public int Y2 { get; set; }
}
