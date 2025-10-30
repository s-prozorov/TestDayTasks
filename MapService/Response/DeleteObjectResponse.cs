using MemoryPack;

namespace MapService.Response;

[MemoryPackable]
public partial class DeleteObjectResponse
{
    public bool? IsSuccess { get; set; }
}