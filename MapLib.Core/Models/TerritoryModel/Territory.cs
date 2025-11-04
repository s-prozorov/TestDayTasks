using MapLib.Core.Models.TileModel;

namespace MapLib.Core.Models.TerritoryModel;

public class Territory
{
    public int Id { get; set; }
    
    public BordersInfo BordersInfo { get; set; }

    public Tile[] Tiles { get; set; } = [];


    public Territory(int id, int x, int y, int width, int height)
    {
        Id = id;
        BordersInfo = new BordersInfo()
        {
            OffsetX = x,
            OffsetY = y,
            Width = width,
            Height = height,
        };
        Tiles = new  Tile[width * height];
    }
}