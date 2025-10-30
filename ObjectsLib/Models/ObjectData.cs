namespace ObjectsLib.Models;

public record ObjectData
{
    public required string Id { get; init; }
    
    public required double X { get; init; }
    
    public required double Y { get; init; }
    
    public required ObjectType Type { get; init; }
    
    public int? OwnerId { get; init; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}