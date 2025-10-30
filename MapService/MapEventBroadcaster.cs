using MagicOnion.Server.Hubs;
using MapService.Events;
using MapService.Interfaces;
using Microsoft.Extensions.Logging;

public class MapEventBroadcaster : IMapEventBroadcaster, IDisposable
{
    private const string GlobalGroup = "map_events";
    private readonly ILogger<MapEventBroadcaster> _logger;
    private IGroup<IMapHubReceiver> _globalGroup;
    private readonly SemaphoreSlim _groupLock = new(1, 1);

    public MapEventBroadcaster(ILogger<MapEventBroadcaster> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Инициализирует глобальную группу. Вызывается из Hub при первом подключении.
    /// </summary>
    public async Task InitializeGroupAsync(HubGroupRepository<IMapHubReceiver> groupRepository)
    {
        if (_globalGroup != null) 
            return;

        await _groupLock.WaitAsync();
        try
        {
            if (_globalGroup == null)
            {
                _globalGroup = await groupRepository.AddAsync(GlobalGroup);
                _logger.LogInformation("Global map events group initialized");
            }
        }
        finally
        {
            _groupLock.Release();
        }
    }

    public Task BroadcastObjectAddedAsync(ObjectAddedEvent ev)
    {
        if (_globalGroup == null)
        {
            _logger.LogWarning("Cannot broadcast: global group not initialized");
            return Task.CompletedTask;
        }

        // .All возвращает receiver proxy для отправки всем в группе
        _globalGroup.All.OnObjectAdded(ev);
        
        _logger.LogDebug(
            "Broadcasted ObjectAdded event for {ObjectId} at ({X},{Y})",
            ev.Map.Id, ev.Map.X, ev.Map.Y);

        return Task.CompletedTask;
    }

    public Task BroadcastObjectUpdatedAsync(ObjectUpdatedEvent ev)
    {
        if (_globalGroup == null)
        {
            _logger.LogWarning("Cannot broadcast: global group not initialized");
            return Task.CompletedTask;
        }

        _globalGroup.All.OnObjectUpdated(ev);
        
        _logger.LogDebug(
            "Broadcasted ObjectUpdated event for {ObjectId}",
            ev.Map.Id);

        return Task.CompletedTask;
    }

    public Task BroadcastObjectDeletedAsync(ObjectDeletedEvent ev)
    {
        if (_globalGroup == null)
        {
            _logger.LogWarning("Cannot broadcast: global group not initialized");
            return Task.CompletedTask;
        }

        _globalGroup.All.OnObjectDeleted(ev);
        
        _logger.LogDebug(
            "Broadcasted ObjectDeleted event for {ObjectId}",
            ev.ObjectId);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _groupLock?.Dispose();
    }
}