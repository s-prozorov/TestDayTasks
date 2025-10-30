using MemoryPack;

namespace MapService.Response.Models;

[MemoryPackable]
public partial class Region
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public int? OwnerId { get; set; }
}