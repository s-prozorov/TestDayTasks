namespace MapObjects;

public interface IMapObjectLayer : IDisposable
{
    Task AddObjectAsync(MapObject obj);
    Task<MapObject?> GetObjectByIdAsync(string id);
    Task<bool> RemoveObjectAsync(string id);
    Task<MapObject?> GetObjectAtPositionAsync(int x, int y);
    Task<IReadOnlyList<MapObject>> GetObjectsInAreaAsync(int x, int y, int width, int height);
    void Subscribe(Action<ObjectEvent> handler);
    void Unsubscribe(Action<ObjectEvent> handler);
}

