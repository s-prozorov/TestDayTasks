using Application.Features.Objects.Models;
using MagicOnion;

namespace Application.Features.Objects.Hubs;

public interface IObjectHub : IStreamingHub<IObjectHub, IObjectHubReceiver>
{
    Task<ObjectInfo[]> GetObjectsInRadiusRequest(short x, short y, short radius);
    Task<long> CreateObject(ObjectInfo objectInfo);
    Task<bool> MoveObject(long id, short x, short y);
    Task<bool> DeleteObject(long id);
}

public interface IObjectHubReceiver
{
    void OnObjectAdded(ObjectInfo objectInfo);
    void OnObjectMoved(long id, short x, short y);
    void OnObjectDeleted(long id);
}