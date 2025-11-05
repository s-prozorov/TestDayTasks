using Application.Features.Objects.Models;
using MagicOnion;

namespace Application.Features.Map.Hubs;

public interface IMapHub : IStreamingHub<IMapHub, IMapHubReceiver>
{
}

public interface IMapHubReceiver
{
}