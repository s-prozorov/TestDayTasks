using MagicOnion;
using MapNetworkService.Contracts;

namespace MapNetworkService.Services;

public interface IMapEventHub : IStreamingHub<IMapEventHub, IMapEventReceiver>
{
    Task JoinAsync();
    Task LeaveAsync();
}

public interface IMapEventReceiver
{
    void OnObjectEvent(ObjectEventDto eventDto);
}

