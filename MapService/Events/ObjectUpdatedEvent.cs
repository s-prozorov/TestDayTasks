using MapService.Response.Models;
using MemoryPack;

namespace MapService.Events;

[MemoryPackable]
public partial class ObjectUpdatedEvent
{
    public MapObject Map { get; set; }
}