using MapService.Response.Models;
using MemoryPack;

namespace MapService.Response;

[MemoryPackable]
public partial class GetRegionsInAreaResponse
{
    public Region[] Regions { get; set; }
}