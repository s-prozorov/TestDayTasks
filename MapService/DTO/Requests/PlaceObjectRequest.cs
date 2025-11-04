using MemoryPack;
using MessagePack;

namespace MapService.DTO.Requests;

[MessagePackObject]
public partial record PlaceObjectRequest
{
    [Key(0)]
    public int X { get; set; }
    
    [Key(1)]
    public int Y { get; set; }
    
    [Key(2)]
    public int ObjectType { get; set; }
};
