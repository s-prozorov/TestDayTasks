using MemoryPack;

namespace MapService.Response;

[MemoryPackable]
public partial class UpdateObjectResponse
{
    public bool? IsSuccess { get; set; }
}