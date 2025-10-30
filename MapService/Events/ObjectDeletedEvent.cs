using MemoryPack;

namespace MapService.Events;

[MemoryPackable]
public partial class ObjectDeletedEvent
{
    public string ObjectId { get; set; }
}