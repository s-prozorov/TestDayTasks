using MagicOnion;

namespace MapSystem.Contracts.Interfaces;

public interface IMapService : IService<IMapService>
{
    UnaryResult<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequest request);
    UnaryResult<GetRegionsInAreaResponse> GetRegionsInAreaAsync(GetRegionsInAreaRequest request);
}