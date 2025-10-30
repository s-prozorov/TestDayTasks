using MemoryPack;

namespace MapService.Response.Models;

[MemoryPackable]
public partial class MapObject
{
    public string Id { get; set; }
    public double? X { get; set; }
    public double? Y { get; set; }
    public int? Type { get; set; }
    public int? OwnerId { get; set; }
}