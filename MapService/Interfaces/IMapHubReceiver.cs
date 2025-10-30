using MapService.Events;

namespace MapService.Interfaces;

public interface IMapHubReceiver
{
    void OnObjectAdded(ObjectAddedEvent ev);
    void OnObjectUpdated(ObjectUpdatedEvent ev);
    void OnObjectDeleted(ObjectDeletedEvent ev);
}