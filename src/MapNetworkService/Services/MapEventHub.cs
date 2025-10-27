using MagicOnion.Server.Hubs;
using MapNetworkService.Contracts;
using MapObjects;

namespace MapNetworkService.Services;

public sealed class MapEventHub : StreamingHubBase<IMapEventHub, IMapEventReceiver>, IMapEventHub
{
    private IGroup<IMapEventReceiver>? _group;
    private readonly IMapObjectLayer _objectLayer;

    public MapEventHub(IMapObjectLayer objectLayer)
    {
        _objectLayer = objectLayer ?? throw new ArgumentNullException(nameof(objectLayer));
    }

    public async Task JoinAsync()
    {
        _group = await Group.AddAsync("map_events");
        _objectLayer.Subscribe(OnObjectChanged);
    }

    public async Task LeaveAsync()
    {
        if (_group != null)
        {
            await _group.RemoveAsync(Context);
        }
    }

    protected override ValueTask OnDisconnected()
    {
        return ValueTask.CompletedTask;
    }

    private void OnObjectChanged(ObjectEvent evt)
    {
        var dto = new ObjectEventDto
        {
            EventType = evt.EventType switch
            {
                MapObjects.ObjectEventType.Added => Contracts.ObjectEventType.Added,
                MapObjects.ObjectEventType.Updated => Contracts.ObjectEventType.Updated,
                MapObjects.ObjectEventType.Deleted => Contracts.ObjectEventType.Deleted,
                _ => throw new ArgumentException($"Unknown event type: {evt.EventType}")
            },
            Object = new MapObjectDto
            {
                Id = evt.Object.Id,
                X = evt.Object.X,
                Y = evt.Object.Y,
                Width = evt.Object.Width,
                Height = evt.Object.Height
            },
            Timestamp = evt.Timestamp
        };

        _group?.All.OnObjectEvent(dto);
    }
}

