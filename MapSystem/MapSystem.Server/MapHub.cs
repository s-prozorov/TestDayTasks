using System.Collections.Concurrent;
using MagicOnion.Server.Hubs;
using MapSystem.Contracts.Interfaces;

namespace MapSystem.Server;

public sealed class MapHub : StreamingHubBase<IMapHub, IMapHubReceiver>, IMapHub
{
    private readonly MapStateManager _stateManager;
    private readonly ConcurrentDictionary<Guid, SubscriptionInfo> _subscriptions = new();
    private IGroup<IMapHubReceiver>? _broadcastGroup;

    public MapHub(MapStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    protected override async ValueTask OnConnecting()
    {
        _broadcastGroup = await Group.AddAsync("MapUpdates");

        _stateManager.SubscribeObjectAdded(evt => { _broadcastGroup?.All.OnObjectAdded(evt); });

        _stateManager.SubscribeObjectUpdated(evt => { _broadcastGroup?.All.OnObjectUpdated(evt); });

        _stateManager.SubscribeObjectDeleted(evt => { _broadcastGroup?.All.OnObjectDeleted(evt); });
    }

    public async Task<bool> SubscribeToAreaAsync(int x, int y, int width, int height)
    {
        try
        {
            var subscription = new SubscriptionInfo(x, y, width, height);
            _subscriptions[ConnectionId] = subscription;

            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }

    public async Task UnsubscribeAsync()
    {
        _subscriptions.TryRemove(ConnectionId, out _);
        await Task.CompletedTask;
    }

    protected override async ValueTask OnDisconnected()
    {
        _subscriptions.TryRemove(ConnectionId, out _);
        await Task.CompletedTask;
    }
}

internal sealed record SubscriptionInfo(int X, int Y, int Width, int Height);