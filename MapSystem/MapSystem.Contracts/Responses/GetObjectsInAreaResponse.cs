using MemoryPack;

namespace MapSystem.Contracts;

[MemoryPackable]
public partial class GetObjectsInAreaResponse
{
    public ObjectData[] Objects { get; set; } = Array.Empty<ObjectData>();
}
