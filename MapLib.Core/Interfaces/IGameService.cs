using MapLib.Core.Models.ObjectModel;
using MapLib.Core.Models.TerritoryModel;

namespace MapLib.Core.Interfaces;

public interface IGameService
{
    Task<(string, ObjectType)> PlaceObject(int x, int y, ObjectType type);

    Task<string> DeleteObject(string objectId);

    Task<List<ObjectInfo>> GetObjectsInArea(int x1, int x2, int y1, int y2);

    List<Territory> GetTerritoriesInArea(int x1, int x2, int y1, int y2);
}