using MagicOnion.Server.Hubs;
using MapService.Interfaces;

namespace MapService;

public class MapGroupManager
{
    private IGroup<IMapHubReceiver>? _globalGroup;
    private readonly object _lock = new();

    public void SetGroup(IGroup<IMapHubReceiver> group)
    {
        lock (_lock)
        {
            _globalGroup = group;
        }
    }

    public IGroup<IMapHubReceiver>? GetGroup()
    {
        lock (_lock)
        {
            return _globalGroup;
        }
    }
}