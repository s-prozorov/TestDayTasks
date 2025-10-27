using MemoryPack;

namespace MapNetworkService.Contracts;

[MemoryPackable]
public partial class GetObjectsInAreaRequest
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

