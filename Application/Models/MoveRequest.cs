namespace Application.Models;

public record struct MoveRequest
{ 
   public int NewX { get; set; }
   public int NewY { get; set; }
}