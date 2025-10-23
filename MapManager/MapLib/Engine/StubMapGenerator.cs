using MapLib.Domain;

namespace MapLib.Engine;

/// <summary>
/// Реализация IMapGenerator, создающая карту-заглушку.
/// </summary>
public class StubMapGenerator : IMapGenerator
{
	private readonly int _territoryCols;
	private readonly int _territoryRows;
	private readonly int _seed;

	public StubMapGenerator(int territoryCols = 10, int territoryRows = 10, int seed = 12345)
	{
		_territoryCols = Math.Max(1, territoryCols);
		_territoryRows = Math.Max(1, territoryRows);
		_seed = seed;
	}

	/// <summary>
	/// Генерирует карту и заполняет массив тайлов и словарь территорий.
	/// </summary>
	/// <param name="map"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <param name="territories"></param>
	public void Generate(long[] map, int width, int height, Dictionary<int, TerritoryInfo> territories)
	{
		var rng = new Random(_seed);

		var total = _territoryCols * _territoryRows;
		for(var id = 1; id <= total; id++)
		{
			territories[id] = new TerritoryInfo { Id = id };
		}

		var blockWidth = Math.Max(1, width / _territoryCols);
		var blockHeight = Math.Max(1, height / _territoryRows);

		for(var y = 0; y < height; y++)
		{
			var territoryRow = Math.Min(y / blockHeight, _territoryRows - 1);

			for(var x = 0; x < width; x++)
			{
				var territoryCol = Math.Min(x / blockWidth, _territoryCols - 1);
				var territoryId = territoryRow * _territoryCols + territoryCol + 1;

				var type = rng.Next(0, 2) == 0 ? TileType.Plain : TileType.Mountain;

				var idx = y * width + x;
				map[idx] = TilePacker.Pack(territoryId, type);
			}
		}
	}
}