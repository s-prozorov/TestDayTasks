using MagicOnion;
using MagicOnion.Server;
using MapSystem.Contracts;
using MapSystem.Contracts.Interfaces;

namespace MapSystem.Server;

public sealed class MapService : ServiceBase<IMapService>, IMapService
{
    private readonly MapStateManager _stateManager;

    public MapService(MapStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async UnaryResult<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var objects = await _stateManager.GetObjectsInAreaAsync(
                request.X,
                request.Y,
                request.Width,
                request.Height);

            return new GetObjectsInAreaResponse
            {
                Objects = objects
            };
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new ReturnStatusException(
                Grpc.Core.StatusCode.InvalidArgument,
                ex.Message);
        }
        catch (Exception ex)
        {
            throw new ReturnStatusException(
                Grpc.Core.StatusCode.Internal,
                $"Ошибка получения объекта: {ex.Message}");
        }
    }

    public UnaryResult<GetRegionsInAreaResponse> GetRegionsInAreaAsync(GetRegionsInAreaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var regions = _stateManager.GetRegionsInArea(
                request.X,
                request.Y,
                request.Width,
                request.Height);

            return UnaryResult.FromResult(new GetRegionsInAreaResponse
            {
                Regions = regions
            });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new ReturnStatusException(
                Grpc.Core.StatusCode.InvalidArgument,
                ex.Message);
        }
        catch (Exception ex)
        {
            throw new ReturnStatusException(
                Grpc.Core.StatusCode.Internal,
                $"Ошибка получения региона: {ex.Message}");
        }
    }
}