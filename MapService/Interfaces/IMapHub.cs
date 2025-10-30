using MagicOnion;
using MapService.Requests;
using MapService.Response;

namespace MapService.Interfaces;

public interface IMapHub : IStreamingHub<IMapHub, IMapHubReceiver>
{
    Task<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequest request);
    Task<GetRegionsInAreaResponse> GetRegionsInAreaAsync(GetRegionsInAreaRequest request);
}