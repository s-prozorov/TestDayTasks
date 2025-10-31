using MagicOnion;
using MapService.Requests;
using MapService.Response;

namespace MapService.Interfaces;

public interface IMapService : IService<IMapService>
{
    UnaryResult<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequest request);
    UnaryResult<GetRegionsInAreaResponse> GetRegionsInAreaAsync(GetRegionsInAreaRequest request);
    UnaryResult<CreateObjectResponse> CreateObjectAsync(CreateObjectRequest request);
    UnaryResult<UpdateObjectResponse> UpdateObjectAsync(UpdateObjectRequest request);
    UnaryResult<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request);
}