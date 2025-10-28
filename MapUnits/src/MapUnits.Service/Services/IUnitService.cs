using MagicOnion;
using MessagePack;
using MapUnits.Service.Contracts;

namespace MapUnits.Service.Services;

public interface IUnitService : IService<IUnitService>
{
    UnaryResult<Nil> StartMovementAsync(StartMovementRequest request);
    UnaryResult<GetUnitsInAreaResponse> GetUnitsInAreaAsync(GetUnitsInAreaRequest request);
}

