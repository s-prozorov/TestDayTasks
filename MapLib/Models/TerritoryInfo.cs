namespace MapLib.Models;

public record TerritoryInfo(int Id, int OwnerId, string Name)
{
    public int Id { get; init; } = Id;
    public int OwnerId { get; init; } = OwnerId;
    public string Name { get; init; } = Name;
}