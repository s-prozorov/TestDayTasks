using MemoryPack;
using MessagePack;

namespace MapService.DTO.Responses;

[MessagePackObject]
public partial record SubscribeResponse
{
    [Key(0)]
    public bool Success { get; set; }
    
    [Key(1)]
    public string SubscriptionId { get; set; } 
    
    [Key(2)]
    public string ErrorMessage { get; set; }
}