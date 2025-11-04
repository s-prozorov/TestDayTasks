using MemoryPack;
using MessagePack;

namespace MapService.DTO.Responses;

[MessagePackObject]
public partial record DeleteObjectResponse
{
    [Key(1)]
    public string ObjectId { get; set; }
}