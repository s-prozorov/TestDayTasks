using Application.Features.Map.Enums;
using Application.Features.Map.Models;

namespace Application.Features.Map.Services;


public class MapService
{
    public Tile[] Map = new Tile[1_000_000];
    public Dictionary<byte, HashSet<uint>> Territories { get; set; }
        = new Dictionary<byte, HashSet<uint>>(87);

    public void GenerateMap()
    {
        byte currentTerritoryId = 0;
        var currentTilesCoordinatesHashSet = new HashSet<uint>();

        for (uint i = 0; i < 1_000_000; i++)
        {
            Map[i] = new Tile
            {
                TerritoryId = currentTerritoryId,
                Type = ((i % 3_000) == 0 && (i % 4) == 0)
                    ? TileTypeEnum.Mountain
                    : TileTypeEnum.Plain
            };

            if (currentTilesCoordinatesHashSet.Count < 11_500)
            {
                currentTilesCoordinatesHashSet.Add(i);
            }
            else
            {
                Territories.Add(currentTerritoryId, currentTilesCoordinatesHashSet);

                currentTilesCoordinatesHashSet.Clear();

                currentTerritoryId++;
            }
        }

        if (currentTilesCoordinatesHashSet.Count > 0)
        {
            currentTilesCoordinatesHashSet.Clear();
        }
    }

    public Tile GetTile(short x, short y)
    {
        return Map[(x * 1000) + y];
    }

    public TileTypeEnum GetTileType(short x, short y)
    {
        return Map[(x * 1000) + y].Type;
    }

    public byte GetTerritoryId(short x, short y)
    {
        return Map[(x * 1000) + y].TerritoryId;
    }

    public TerritoryInfo GetTerritoryInfo(byte id)
    {
        var coordinatesHashSet = Territories[id];

        var territoryInfo = new TerritoryInfo
        {
            Id = id,
            Tiles = new Tile[coordinatesHashSet.Count],
        };

        var idx = 0;
        foreach (var i in coordinatesHashSet)
        {
            territoryInfo.Tiles[idx] = Map[i];
            idx++;
        }

        return territoryInfo;
    }
}