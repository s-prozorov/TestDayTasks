using MapLib.Core.Interfaces;
using MapLib.Core.Models.ObjectModel;
using MapLib.Core.Models.TerritoryModel;

namespace MapLib.Services;

public class GameService
(
    IMapService mapService,
    IObjectService objectService) : IGameService
{
    public async Task<(string, ObjectType)> PlaceObject(int x, int y, ObjectType type)
    {
        var tileType = mapService.GetTileType(x, y);
        // TODO: проверка типа тайла и типа объекта
        
        var nearbyObjects = await objectService.GetObjects(x, y, 5);
        if (nearbyObjects.Any(o => o.X == x && o.Y == y))
            throw new ArgumentException("Position is occupied");

        var objectId = await objectService.CreateObject(x, y, type);
        return (objectId, type);
    }

    public async Task<string> DeleteObject(string objectId)
    {
        if (await objectService.DeleteObject(objectId))
            return objectId;

        return string.Empty;
    }

    public async Task<List<ObjectInfo>> GetObjectsInArea(int x1, int x2, int y1, int y2)
    {
        return await objectService.GetObjectsInArea(x1, x2, y1, y2);
    }

    public List<Territory> GetTerritoriesInArea(int x1, int x2, int y1, int y2)
    {
        return mapService.GetTerritoriesInArea(x1, x2, y1, y2);
    }
}