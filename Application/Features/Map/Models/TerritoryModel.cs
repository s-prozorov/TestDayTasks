namespace Application.Features.Map.Models;

public class TerritoryInfo
{
    public byte Id { get; set; }
    public Tile[] Tiles { get; set; } = new Tile[0];
}