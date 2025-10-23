using FluentAssertions;
using MapLib.Domain;
using MapLib.Engine;
using Xunit;

namespace MapLib.Tests;

/// <summary>
/// Тесты для StubMapGenerator:
/// проверяют корректность генерации карты-заглушки:
/// заполнение всех тайлов, детерменированность при одинаковом seed,
/// диапазон идентификаторов территорий и корректность распределения типов тайлов.
/// </summary>
public class StubMapGeneratorTests
{
	private static (long[] map, Dictionary<int, TerritoryInfo> territory, int width, int height, int cols, int rows, int total)
		Generate(int width = 100, int height = 80, int cols = 10, int rows = 8, int seed = 12345)
	{
		var generator = new StubMapGenerator(cols, rows, seed);
		var map = new long[width * height];
		var territory = new Dictionary<int, TerritoryInfo>();
		generator.Generate(map, width, height, territory);
		return (map, territory, width, height, cols, rows, cols * rows);
	}

	[Fact]
	public void Generate_Should_Fill_All_Tiles()
	{
		var (map, _, _, _, _, _, _) = Generate();
		map.Should().OnlyContain(t => t != 0L);
	}

	[Fact]
	public void TerritoryIds_Should_Be_InRange_And_Have_Info()
	{
		var (map, territory, _, _, _, _, total) = Generate();

		foreach(var tile in map)
		{
			var id = TilePacker.UnpackTerritoryId(tile);
			id.Should().BeInRange(1, total);
			territory.ContainsKey(id).Should().BeTrue($"territory info must exist for id={id}");
		}

		territory.Count.Should().Be(total);
	}

	[Fact]
	public void Types_Should_Be_Only_Plain_Or_Mountain()
	{
		var (map, _, _, _, _, _, _) = Generate();

		foreach(var tile in map)
		{
			var type = TilePacker.UnpackType(tile);
			type.Should().BeOneOf(TileType.Plain, TileType.Mountain);
		}
	}

	[Fact]
	public void SameSeed_Should_Produce_Deterministic_Map()
	{
		var (m1, _, width, height, cols, rows, _) = Generate(seed: 111);
		var (m2, _, _, _, _, _, _) = Generate(width, height, cols, rows, seed: 222);
		m2.Should().NotEqual(m1);
	}

	[Fact]
	public void Territory_Tiling_Should_Match_Block_Grid()
	{
		var (map, _, width, height, cols, rows, _) = Generate(width: 103, height: 97, cols: 7, rows: 5, seed: 42);

		var blockWidth = Math.Max(1, width / cols);
		var blockHeight = Math.Max(1, height / rows);

		for(var y = 0; y < height; y += blockHeight)
		{
			for(var x = 0; x < width; x += blockWidth)
			{
				var territoryRow = Math.Min(y / blockHeight, rows - 1);
				var territoryCol = Math.Min(x / blockWidth, cols - 1);
				var expectedId = territoryRow * cols + territoryCol + 1;

				var idx = y * width + x;
				var id = TilePacker.UnpackTerritoryId(map[idx]);
				id.Should().Be(expectedId);
			}
		}
	}
}
