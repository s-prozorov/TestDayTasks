namespace TASK_1_1.Entities;

struct Tile
{
    public TileEnum Type;

    public bool CanPlaceObject => Type == TileEnum.Plain;
}