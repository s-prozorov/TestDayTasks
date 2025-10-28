using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class ObjectEventDto
{
    public ObjectEventType EventType { get; set; }
    public MapObjectDto Object { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public enum ObjectEventType
{
    Added,
    Updated,
    Deleted
}

