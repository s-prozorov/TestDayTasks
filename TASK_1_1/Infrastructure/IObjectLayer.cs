namespace TASK_1_1.Infrastructure;
public interface IObjectLayer
{
    void AddObject(MapObject obj);
    void RemoveObject(string id);
    MapObject? GetObjectById(string id);
    MapObject? GetObjectAt(int x, int y);
    IEnumerable<MapObject> GetObjectsInArea(int startX, int startY, int endX, int endY);

    event Action<MapObject> ObjectAdded;
    event Action<MapObject> ObjectUpdated;
    event Action<MapObject> ObjectRemoved;
}
