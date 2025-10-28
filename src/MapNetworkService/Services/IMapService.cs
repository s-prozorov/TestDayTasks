using MagicOnion;
using MapNetworkService.Contracts;

namespace MapNetworkService.Services;

public interface IMapService : IService<IMapService>
{
    UnaryResult<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequest request);
    UnaryResult<GetRegionsInAreaResponse> GetRegionsInAreaAsync(GetRegionsInAreaRequest request);
    UnaryResult<GetUnitsInAreaResponse> GetUnitsInAreaAsync(GetObjectsInAreaRequest request);
    UnaryResult StartUnitMovementAsync(StartMovementRequest request);
}

