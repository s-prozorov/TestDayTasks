using MemoryPack;

namespace MapSystem.Contracts;

[MemoryPackable]
public partial class ObjectData
{
    public string Id { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}