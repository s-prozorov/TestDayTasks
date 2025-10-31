using MapService.Events;
using MapService.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace MapService;

public class MapEventBroadcaster : IMapEventBroadcaster
{
    private readonly ILogger<MapEventBroadcaster> _logger;
    private readonly Channel<object> _eventChannel;

    public MapEventBroadcaster(ILogger<MapEventBroadcaster> logger)
    {
        _logger = logger;
        // Создаем unbounded channel для событий
        _eventChannel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
        {
            SingleReader = false, // Может быть несколько читателей (каждый hub instance)
            SingleWriter = false  // Может быть несколько писателей
        });
    }

    public ChannelReader<object> Events => _eventChannel.Reader;

    public async Task BroadcastObjectAddedAsync(ObjectAddedEvent ev)
    {
        await _eventChannel.Writer.WriteAsync(ev);

        _logger.LogDebug(
            "Published ObjectAdded event for {ObjectId} at ({X},{Y})",
            ev.Map.Id, ev.Map.X, ev.Map.Y);
    }

    public async Task BroadcastObjectUpdatedAsync(ObjectUpdatedEvent ev)
    {
        await _eventChannel.Writer.WriteAsync(ev);

        _logger.LogDebug(
            "Published ObjectUpdated event for {ObjectId}",
            ev.Map.Id);
    }

    public async Task BroadcastObjectDeletedAsync(ObjectDeletedEvent ev)
    {
        await _eventChannel.Writer.WriteAsync(ev);

        _logger.LogDebug(
            "Published ObjectDeleted event for {ObjectId}",
            ev.ObjectId);
    }
}