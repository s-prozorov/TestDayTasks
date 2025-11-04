using MemoryPack;
using MessagePack;

namespace MapService.DTO.Requests;

[MessagePackObject]
public partial record DeleteObjectRequest
{
    [Key(1)]
    public string ObjectId { get; set; }
}