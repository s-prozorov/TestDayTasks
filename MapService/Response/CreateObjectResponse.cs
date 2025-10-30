using MemoryPack;

namespace MapService.Response;

[MemoryPackable]
public partial class CreateObjectResponse
{
    public bool? IsSuccess { get; set; } 
    
    public string ObjectId { get; set; }
}