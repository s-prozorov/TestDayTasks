using MagicOnion.Server.Hubs;
using MapService.Interfaces;
using MapService.Requests;
using MapService.Response;
using Microsoft.Extensions.Logging;

namespace MapService;

public class MapHub : StreamingHubBase<IMapHub, IMapHubReceiver>, IMapHub
{
    private const string GlobalGroup = "map_events";
    private readonly ILogger<MapHub> _logger;
    private readonly IMapService _mapService;
    private readonly MapGroupManager _groupManager;

    public MapHub(
        ILogger<MapHub> logger,
        IMapService mapService,
        MapGroupManager groupManager)
    {
        _logger = logger;
        _mapService = mapService;
        _groupManager = groupManager;
    }

    protected override async ValueTask OnConnecting()
    {
        // Добавляем клиента в глобальную группу
        var group = await Group.AddAsync(GlobalGroup);

        // Регистрируем группу в менеджере (для broadcast из фонового сервиса)
        _groupManager.SetGroup(group);

        _logger.LogInformation(
            "Client {ConnectionId} connected and joined group {GroupName}",
            ConnectionId, GlobalGroup);
    }

    protected override ValueTask OnDisconnected()
    {
        _logger.LogInformation(
            "Client {ConnectionId} disconnected",
            ConnectionId);

        return CompletedTask;
    }

    public async Task<GetObjectsInAreaResponse> GetObjectsInAreaAsync(
        GetObjectsInAreaRequest request)
    {
        return await _mapService.GetObjectsInAreaAsync(request);
    }

    public async Task<GetRegionsInAreaResponse> GetRegionsInAreaAsync(
        GetRegionsInAreaRequest request)
    {
        return await _mapService.GetRegionsInAreaAsync(request);
    }
}