using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using MapUnits.Core;
using MapUnits.Service.Contracts;

namespace MapUnits.Service.Services;

public class UnitService : ServiceBase<IUnitService>, IUnitService
{
    private readonly IUnitMovementService _movementService;

    public UnitService(IUnitMovementService movementService)
    {
        _movementService = movementService;
    }

    public UnaryResult<Nil> StartMovementAsync(StartMovementRequest request)
    {
        try
        {
            _movementService.StartMovement(request.UnitId, request.TargetX, request.TargetY);
            return UnaryResult.FromResult(Nil.Default);
        }
        catch (Exception ex)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, ex.Message);
        }
    }

    public UnaryResult<GetUnitsInAreaResponse> GetUnitsInAreaAsync(GetUnitsInAreaRequest request)
    {
        var units = _movementService.GetUnitsInArea(request.X, request.Y, request.Width, request.Height);
        var movements = _movementService.GetMovementsInArea(request.X, request.Y, request.Width, request.Height);

        var unitDtos = new UnitDto[units.Count];
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            unitDtos[i] = new UnitDto { Id = u.Id, X = u.X, Y = u.Y };
        }

        var movementDtos = new MovementDto[movements.Count];
        for (int i = 0; i < movements.Count; i++) {
            var m = movements[i];
            movementDtos[i] = new MovementDto
            {
                UnitId = m.UnitId,
                StartX = m.StartX,
                StartY = m.StartY,
                TargetX = m.TargetX,
                TargetY = m.TargetY,
                Speed = m.Speed
            };
        }

        return UnaryResult.FromResult(new GetUnitsInAreaResponse { Units = unitDtos, Movements = movementDtos });
    }
}

