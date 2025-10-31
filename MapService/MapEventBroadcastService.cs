using MagicOnion.Server.Hubs;
using MapService.Events;
using MapService.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MapService;

/// <summary>
/// Фоновый сервис для трансляции событий из канала в StreamingHub группы
/// </summary>
public class MapEventBroadcastService : BackgroundService
{
    private readonly ILogger<MapEventBroadcastService> _logger;
    private readonly MapEventBroadcaster _broadcaster;
    private readonly MapGroupManager _groupManager;

    public MapEventBroadcastService(
        ILogger<MapEventBroadcastService> logger,
        IMapEventBroadcaster broadcaster,
        MapGroupManager groupManager)
    {
        _logger = logger;
        _broadcaster = (MapEventBroadcaster)broadcaster;
        _groupManager = groupManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MapEventBroadcastService started");

        try
        {
            await foreach (var ev in _broadcaster.Events.ReadAllAsync(stoppingToken))
            {
                _logger.LogDebug("Received event from channel: {EventType}", ev.GetType().Name);

                var group = _groupManager.GetGroup();
                if (group == null)
                {
                    _logger.LogWarning("No active group, skipping event {EventType}", ev.GetType().Name);
                    continue;
                }

                try
                {
                    switch (ev)
                    {
                        case ObjectAddedEvent addedEvent:
                            group.All.OnObjectAdded(addedEvent);
                            _logger.LogDebug("Broadcasted ObjectAdded to group: {ObjectId}", addedEvent.Map.Id);
                            break;

                        case ObjectUpdatedEvent updatedEvent:
                            group.All.OnObjectUpdated(updatedEvent);
                            _logger.LogDebug("Broadcasted ObjectUpdated to group: {ObjectId}", updatedEvent.Map.Id);
                            break;

                        case ObjectDeletedEvent deletedEvent:
                            group.All.OnObjectDeleted(deletedEvent);
                            _logger.LogDebug("Broadcasted ObjectDeleted to group: {ObjectId}", deletedEvent.ObjectId);
                            break;

                        default:
                            _logger.LogWarning("Unknown event type: {EventType}", ev.GetType().Name);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error broadcasting event");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("MapEventBroadcastService stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MapEventBroadcastService failed");
        }
    }
}