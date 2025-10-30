using MapService.Requests.Base;
using MemoryPack;

namespace MapService.Requests;

[MemoryPackable]
public partial class DeleteObjectRequest 
{
    public string ObjectId { get; set; }
}