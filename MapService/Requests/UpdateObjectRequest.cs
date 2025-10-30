using MapService.Requests.Base;
using MemoryPack;

namespace MapService.Requests;

[MemoryPackable]
public partial class UpdateObjectRequest
{
    public string ObjectId { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
}