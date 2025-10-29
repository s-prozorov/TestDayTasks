using MapSystem.Contracts.Events;

namespace MapSystem.Contracts.Interfaces;

public interface IMapHubReceiver
{
    void OnObjectAdded(ObjectAddedEvent evt);
    void OnObjectUpdated(ObjectUpdatedEvent evt);
    void OnObjectDeleted(ObjectDeletedEvent evt);
}