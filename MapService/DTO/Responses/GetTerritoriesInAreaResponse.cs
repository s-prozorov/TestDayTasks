using MapLib.Core.Models.TerritoryModel;
using MemoryPack;
using MessagePack;

namespace MapService.DTO.Responses;

[MessagePackObject]
public partial class GetTerritoriesInAreaResponse
{
    [Key(0)]
    public List<TerritoryItem> Items { get; set; }
    
    [MessagePackObject]
    public partial record TerritoryItem
    {
        [Key(0)]
        public int Id { get; set; }
        
        [Key(1)]
        public BordersInfoItem BordersInfo { get; set; }
        
        [MessagePackObject]
        public partial record BordersInfoItem
        {       
            [Key(0)]
            public int OffsetX { get; set; }
            
            [Key(1)]
            public int OffsetY { get; set; }
            
            [Key(3)]
            public int Width { get; set; }
            
            [Key(4)]
            public int Height { get; set; }
        }
    }
}
