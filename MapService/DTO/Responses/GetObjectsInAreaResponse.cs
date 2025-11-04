using MemoryPack;
using MessagePack;

namespace MapService.DTO.Responses;

[MessagePackObject]
public partial record GetObjectsInAreaResponse
{
    [Key(0)]
    public List<ObjectItem> Items { get; set; }
    
    [MessagePackObject]
    public partial record ObjectItem
    {
        [Key(0)]
        public string Id { get; set; }
        
        [Key(1)]
        public int X { get; set; }
        
        [Key(2)]
        public int Y { get; set; }
        
        [Key(3)]
        public int Type { get; set; }
    }
}