using Cysharp.Runtime.Multicast;
using MagicOnion.Server.Hubs;

namespace Application.Features.Map.Hubs;


public class MapHubService : StreamingHubBase<IMapHub, IMapHubReceiver>, IMapHub
{
    private readonly IMulticastSyncGroup<Guid, IMapHubReceiver> _group;

    public MapHubService(IMulticastGroupProvider groupProvider)
    {
        _group = groupProvider.GetOrAddSynchronousGroup<Guid, IMapHubReceiver>("Game");

    }

    protected override ValueTask OnConnected()
    {
        _group.Add(ConnectionId, Client);

        return base.OnConnected();
    }
}