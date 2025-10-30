using MemoryPack;

namespace MapService.Requests.Base;

[MemoryPackable]
public partial class RectangleAreaRequest
{
    public int? X1 { get; set; }
    public int? Y1 { get; set; }
    public int? X2 { get; set; }
    public int? Y2 { get; set; }
}