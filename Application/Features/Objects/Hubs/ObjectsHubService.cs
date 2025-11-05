using Application.Features.Objects.Models;
using Application.Features.Objects.Services;
using Cysharp.Runtime.Multicast;
using MagicOnion.Server.Hubs;

namespace Application.Features.Objects.Hubs;

public class ObjectsHubService : StreamingHubBase<IObjectHub, IObjectHubReceiver>, IObjectHub
{
    private readonly IMulticastSyncGroup<Guid, IObjectHubReceiver> _group;

    private readonly ObjectsService _objectsService;
    public ObjectsHubService(ObjectsService objectsService, IMulticastGroupProvider groupProvider)
    {
        _objectsService = objectsService;
        _group = groupProvider.GetOrAddSynchronousGroup<Guid, IObjectHubReceiver>("Game");

    }

    protected override ValueTask OnConnected()
    {
        _group.Add(ConnectionId, Client);

        return base.OnConnected();
    }

    public async Task<long> CreateObject(ObjectInfo objectInfo)
    {
        var id = await _objectsService.CreateObject(objectInfo);

        _group.All.OnObjectAdded(objectInfo);

        return id;
    }
    public async Task<bool> MoveObject(long id, short x, short y)
    {
        var isMoved = await _objectsService.MoveObject(id, x, y);
        if (isMoved)
        {
            _group.All.OnObjectMoved(id, x, y);
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> DeleteObject(long id)
    {
        var isDeleted = await _objectsService.DeleteObject(id);
        if (isDeleted)
        {
            _group.All.OnObjectDeleted(id);
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<ObjectInfo[]> GetObjectsInRadiusRequest(short x, short y, short radius)
    {
        return await _objectsService.GetObjectsInZone(new CenterModel { X = x, Y = y }, radius);
    }
}