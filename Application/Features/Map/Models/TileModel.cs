using Application.Features.Map.Enums;

namespace Application.Features.Map.Models;

public class Tile
{
    public TileTypeEnum Type { get; set; }
    public byte TerritoryId { get; set; }
}