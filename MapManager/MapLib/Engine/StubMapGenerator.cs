using MapLib.Domain;

namespace MapLib.Engine;

/// <summary>
/// Реализация IMapGenerator, создающая карту-заглушку.
/// </summary>
public class StubMapGenerator : IMapGenerator
{
	private readonly int _regionCols;
	private readonly int _regionRows;
	private readonly int _seed;

	public StubMapGenerator(int regionCols = 10, int regionRows = 10, int seed = 12345)
	{
		_regionCols = Math.Max(1, regionCols);
		_regionRows = Math.Max(1, regionRows);
		_seed = seed;
	}

	/// <summary>
	/// Генерирует карту и заполняет массив тайлов и словарь территорий.
	/// </summary>
	public void Generate(long[] map, int width, int height, Dictionary<int, RegionInfo> regions)
	{
		var rng = new Random(_seed);

		var total = _regionCols * _regionRows;
		for(var id = 1; id <= total; id++)
		{
			regions[id] = new RegionInfo
			{
			    Id = id,
				Name = $"Region {id}"
			};
		}

		var blockWidth = Math.Max(1, width / _regionCols);
		var blockHeight = Math.Max(1, height / _regionRows);

		for(var y = 0; y < height; y++)
		{
			var regionRow = Math.Min(y / blockHeight, _regionRows - 1);

			for(var x = 0; x < width; x++)
			{
				var regionCol = Math.Min(x / blockWidth, _regionCols - 1);
				var regionId = regionRow * _regionCols + regionCol + 1;

				var type = rng.Next(0, 2) == 0 ? TileType.Plain : TileType.Mountain;

				var idx = y * width + x;
				map[idx] = TilePacker.Pack(regionId, type);
			}
		}
	}
}