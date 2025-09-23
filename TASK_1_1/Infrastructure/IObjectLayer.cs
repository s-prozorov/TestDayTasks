
using TASK_1_1.Entities;

namespace TASK_1_1.Infrastructure;
public interface IObjectLayer
{
    void AddObject(MapObject obj);
    void UpdateObject(MapObject obj);
    void RemoveObject(ulong id);

    MapObject? GetObjectById(ulong id);
    MapObject? GetObjectByCoordinate(int x, int y);
    IEnumerable<MapObject> GetObjectsInArea(int startX, int startY, int endX, int endY);

    event Action<MapObject> ObjectAdded;
    event Action<MapObject> ObjectUpdated;
    event Action<MapObject> ObjectDeleted;
}
