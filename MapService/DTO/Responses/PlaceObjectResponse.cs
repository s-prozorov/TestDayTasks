using MemoryPack;
using MessagePack;

namespace MapService.DTO.Responses;

[MessagePackObject]
public partial record PlaceObjectResponse
{
    [Key(0)]
    public string ObjectId { get; set; }
    
    [Key(1)]
    public int ObjectType { get; set; }
}