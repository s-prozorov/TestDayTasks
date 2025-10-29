using MemoryPack;

namespace MapSystem.Contracts.Events;

[MemoryPackable]
public partial class ObjectDeletedEvent
{
    public uint ObjectId { get; set; }
}