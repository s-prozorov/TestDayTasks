using MemoryPack;

namespace MapSystem.Contracts.Events;

[MemoryPackable]
public partial class ObjectAddedEvent
{
    public ObjectData Object { get; set; } = new();
}