namespace Application.Models;

public record struct ObjectCreationRequest
{
    public int X { get; set; }
     public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}