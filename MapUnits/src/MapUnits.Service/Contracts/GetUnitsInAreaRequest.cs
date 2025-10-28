using MessagePack;

namespace MapUnits.Service.Contracts;

[MessagePackObject]
public class GetUnitsInAreaRequest
{
    [Key(0)]
    public int X { get; set; }
    
    [Key(1)]
    public int Y { get; set; }
    
    [Key(2)]
    public int Width { get; set; }
    
    [Key(3)]
    public int Height { get; set; }
}


