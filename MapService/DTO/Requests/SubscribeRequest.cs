using MessagePack;

namespace MapService.DTO.Requests;

[MessagePackObject]
public partial record SubscribeRequest
{
    [Key(0)]
    public List<string> EventTypes { get; set; }  = new List<string>();
}