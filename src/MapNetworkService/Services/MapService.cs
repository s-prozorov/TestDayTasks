using MagicOnion;
using MagicOnion.Server;
using MapNetworkService.Contracts;
using MapObjects;
using MapRegions;

namespace MapNetworkService.Services;

public sealed class MapService : ServiceBase<IMapService>, IMapService
{
    private readonly IMapObjectLayer _objectLayer;
    private readonly IRegionLayer _regionLayer;

    public MapService(IMapObjectLayer objectLayer, IRegionLayer regionLayer)
    {
        _objectLayer = objectLayer ?? throw new ArgumentNullException(nameof(objectLayer));
        _regionLayer = regionLayer ?? throw new ArgumentNullException(nameof(regionLayer));
    }

    public async UnaryResult<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequest request)
    {
        try
        {
            var objects = await _objectLayer.GetObjectsInAreaAsync(
                request.X, 
                request.Y, 
                request.Width, 
                request.Height);

            var dtos = objects.Select(obj => new MapObjectDto
            {
                Id = obj.Id,
                X = obj.X,
                Y = obj.Y,
                Width = obj.Width,
                Height = obj.Height
            }).ToArray();

            return new GetObjectsInAreaResponse { Objects = dtos };
        }
        catch (Exception ex)
        {
            throw new ReturnStatusException(
                Grpc.Core.StatusCode.InvalidArgument, 
                $"Error getting objects in area: {ex.Message}");
        }
    }

    public UnaryResult<GetRegionsInAreaResponse> GetRegionsInAreaAsync(GetRegionsInAreaRequest request)
    {
        try
        {
            var regionIds = _regionLayer.GetRegionsInArea(
                request.X, 
                request.Y, 
                request.Width, 
                request.Height);

            var dtos = regionIds.Select(id =>
            {
                var metadata = _regionLayer.GetRegionMetadata(id);
                return new RegionDto
                {
                    Id = metadata.Id,
                    Name = metadata.Name
                };
            }).ToArray();

            return UnaryResult.FromResult(new GetRegionsInAreaResponse { Regions = dtos });
        }
        catch (Exception ex)
        {
            throw new ReturnStatusException(
                Grpc.Core.StatusCode.InvalidArgument, 
                $"Error getting regions in area: {ex.Message}");
        }
    }
}

