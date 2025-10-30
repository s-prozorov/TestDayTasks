using MagicOnion.Server.Hubs;
using MapService.Events;

namespace MapService.Interfaces;

public interface IMapEventBroadcaster
{
    Task InitializeGroupAsync(HubGroupRepository<IMapHubReceiver> groupRepository);
    Task BroadcastObjectAddedAsync(ObjectAddedEvent ev);
    Task BroadcastObjectUpdatedAsync(ObjectUpdatedEvent ev);
    Task BroadcastObjectDeletedAsync(ObjectDeletedEvent ev);
}