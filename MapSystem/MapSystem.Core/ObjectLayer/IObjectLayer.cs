namespace MapSystem.Core.ObjectLayer;

/// <summary>
///     Интерфейс для работы со слоем объектов карты.
/// </summary>
public interface IObjectLayer
{
    Task<uint> AddAsync(int x, int y, int width, int height);
    Task<MapObject?> GetByIdAsync(uint id);
    Task<MapObject?> GetAtPositionAsync(int x, int y);
    Task<IReadOnlyCollection<MapObject>> GetInAreaAsync(int x, int y, int width, int height);
    Task<bool> UpdateAsync(uint id, int newX, int newY, int newWidth, int newHeight);

    Task<bool> IsInAreaAsync(uint objectId, int x, int y, int width, int height);
    Task<bool> RemoveAsync(uint id);

    event Action<MapObject>? ObjectAdded;
    event Action<MapObject>? ObjectUpdated;
    event Action<uint>? ObjectDeleted;
}