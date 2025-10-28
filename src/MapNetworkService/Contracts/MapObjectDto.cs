using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class MapObjectDto
{
    public string Id { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

