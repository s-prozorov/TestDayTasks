using MagicOnion;

namespace MapSystem.Contracts.Interfaces;

public interface IMapHub : IStreamingHub<IMapHub, IMapHubReceiver>
{
    Task<bool> SubscribeToAreaAsync(int x, int y, int width, int height);
    Task UnsubscribeAsync();
}