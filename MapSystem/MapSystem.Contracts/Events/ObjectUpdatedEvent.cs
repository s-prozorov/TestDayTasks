using MemoryPack;

namespace MapSystem.Contracts.Events;

[MemoryPackable]
public partial class ObjectUpdatedEvent
{
    public ObjectData Object { get; set; } = new();
}