using MagicOnion.Server.Hubs;
using MapService.Interfaces;
using MapService.Requests;
using MapService.Response;
using Microsoft.Extensions.Logging;

public class MapHub : StreamingHubBase<IMapHub, IMapHubReceiver>, IMapHub
{
    private const string GlobalGroup = "map_events";
    private readonly ILogger<MapHub> _logger;
    private readonly IMapService _mapService;
    private readonly IMapEventBroadcaster _objectEventPublisher;

    public MapHub(ILogger<MapHub> logger, 
        IMapService mapService, 
        IMapEventBroadcaster objectEventPublisher)
    {
        _logger = logger;
        _mapService = mapService;
        _objectEventPublisher = objectEventPublisher;
    }

    protected override async ValueTask OnConnecting()
    {
        await _objectEventPublisher.InitializeGroupAsync(Group);
        await Group.AddAsync(GlobalGroup);

        _logger.LogInformation(
            "Client {ConnectionId} connected",
            ConnectionId);
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