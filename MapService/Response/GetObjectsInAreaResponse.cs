using MapService.Response.Models;
using MemoryPack;

namespace MapService.Response;

[MemoryPackable]
public partial class GetObjectsInAreaResponse
{
    public MapObject[] Objects { get; set; }
}