using MemoryPack;

namespace MapService.Requests;

[MemoryPackable]
public partial class CreateObjectRequest
{
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? ObjectTypeId { get; set; }
}