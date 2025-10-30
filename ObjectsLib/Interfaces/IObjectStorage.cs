using ObjectsLib.Models;

namespace ObjectsLib.Interfaces;

public interface IObjectStorage
{
    Task<ObjectData> CreateObjectAsync(ObjectData objectData);
    
    Task<bool> DeleteObjectAsync(string id);
        
    Task<ObjectData?> GetObjectAsync(string id);
    
    Task<ObjectData> UpdateObjectAsync(ObjectData objectData);
        
    Task<IReadOnlyList<ObjectData>> GetObjectsInZoneAsync(
        double centerX, 
        double centerY, 
        double radiusKm);
        
    Task<bool> ExistsAsync(string id);
}