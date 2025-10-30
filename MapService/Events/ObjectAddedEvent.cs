using MapService.Response.Models;
using MemoryPack;

namespace MapService.Events;

[MemoryPackable]
public partial class ObjectAddedEvent
{
    public MapObject Map { get; set; }
}
