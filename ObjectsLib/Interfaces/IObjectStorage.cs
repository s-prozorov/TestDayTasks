using ObjectsLib.Models;

namespace ObjectsLib.Interfaces;

public interface IObjectStorage
{
    Task<ObjectData> CreateObjectAsync(ObjectData objectData, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteObjectAsync(string id, CancellationToken cancellationToken = default);
        
    Task<ObjectData?> GetObjectAsync(string id, CancellationToken cancellationToken = default);
        
    Task<IReadOnlyList<ObjectData>> GetObjectsInZoneAsync(
        double centerX, 
        double centerY, 
        double radiusKm, 
        CancellationToken cancellationToken = default);
        
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}