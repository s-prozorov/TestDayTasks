using FluentAssertions;
using MapLib.Domain;
using MapLib.Engine;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MapLib.Tests;

/// <summary>
/// тесты для SurfaceMap:
/// проверяют корректность чтения/записи тайлов, загрузки данных, массовых операций над областью,
/// сохранение идентификаторов территорий, а также валидацию входных данных.
/// </summary>
public class SurfaceMapTests
{
	private static SurfaceMap CreateSurface(int width = 16, int height = 12, int cols = 4, int rows = 3, int seed = 12345)
	{
		var generator = new StubMapGenerator(cols, rows, seed);
		return new SurfaceMap(width, height, generator, NullLogger<SurfaceMap>.Instance);
	}

	[Fact]
	public void Constructor_Should_Throw_On_NonPositive_Dimensions()
	{
		var generator = new StubMapGenerator();
		var width0 = () => new SurfaceMap(0, 10, generator);
		var height0 = () => new SurfaceMap(10, 0, generator);
		width0.Should().Throw<ArgumentOutOfRangeException>();
		height0.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Dimensions_Should_Match_Constructor_Arguments()
	{
		var surface = CreateSurface(7, 9);
		surface.Width.Should().Be(7);
		surface.Height.Should().Be(9);
	}

	[Fact]
	public void GetTileType_And_GetTerritoryId_Should_Work_For_Corners()
	{
		var surface = CreateSurface(8, 5);
		surface.Invoking(_ => _.GetTileType(0, 0)).Should().NotThrow();
		surface.Invoking(_ => _.GetTileType(7, 4)).Should().NotThrow();
		surface.Invoking(_ => _.GetTerritoryId(0, 0)).Should().NotThrow();
		surface.Invoking(_ => _.GetTerritoryId(7, 4)).Should().NotThrow();
	}

	[Fact]
	public void Public_Method_Should_Validate_Range()
	{
		var surface = CreateSurface(4, 4);
		surface.Invoking(_ => _.GetTileType(-1, 0)).Should().Throw<ArgumentOutOfRangeException>();
		surface.Invoking(_ => _.GetTileType(4, 0)).Should().Throw<ArgumentOutOfRangeException>();
		surface.Invoking(_ => _.GetTerritoryId(0, -1)).Should().Throw<ArgumentOutOfRangeException>();
		surface.Invoking(_ => _.SetTileType(0, 4, TileType.Plain)).Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void SetTileType_Should_Update_Type_And_Preserve_TerritoryId()
	{
		var surface = CreateSurface(10, 6);
		var x = 3;
		var y = 2;

		var beforeId = surface.GetTerritoryId(x, y);
		var beforeType = surface.GetTileType(x, y);

		var newType = beforeType == TileType.Plain ? TileType.Mountain : TileType.Plain;
		surface.SetTileType(x, y, newType);

		surface.GetTileType(x, y).Should().Be(newType);
		surface.GetTerritoryId(x, y).Should().Be(beforeId);
	}

	[Fact]
	public void LoadForm_Should_Replace_Map_Data_And_Validate_Length()
	{
		var surface = CreateSurface(5, 5);
		var area = surface.Width * surface.Height;

		var tiles = new long[area];
		for(var i = 0; i < area; i++)
		{
			tiles[i] = TilePacker.Pack(1, TileType.Plain);
		}

		surface.LoadFrom(tiles);

		surface.GetTerritoryId(0, 0).Should().Be(1);
		surface.GetTileType(4, 4).Should().Be(TileType.Plain);

		var wrong = new long[area - 1];
		surface.Invoking(_ => _.LoadFrom(wrong)).Should().Throw<ArgumentException>();
	}

	[Fact]
	public void FillArea_Should_Set_Type_For_Rectangle_And_Preserve_TerritoryIds()
	{
		var surface = CreateSurface(12, 8);
		int x = 2, y = 1, width = 5, height = 3;

		var idsBefore = new Dictionary<int, int>();
		for(var tileY = y; tileY < y + height; tileY++)
		{
			for(var tileX = x; tileX < x + width; tileX++)
			{
				idsBefore[tileY * surface.Width + tileX] = surface.GetTerritoryId(tileX, tileY);
			}
		}

		surface.FillArea(x, y, width, height, TileType.Mountain);

		for(var tileY = y; tileY < y + height; tileY++)
		{
			for(var tileX = x; tileX < x + width; tileX++)
			{
				surface.GetTileType(tileX, tileY).Should().Be(TileType.Mountain);
				var idx = tileY * surface.Width + tileX;
				surface.GetTerritoryId(tileX, tileY).Should().Be(idsBefore[idx]);
			}
		}
	}

	[Fact]
	public void FillArea_Should_Clip_To_Bounds_And_Not_Throw()
	{
		var surface = CreateSurface(10, 10);
		surface.Invoking(_ => _.FillArea(-5, -5, 8, 8, TileType.Plain)).Should().NotThrow();
		surface.Invoking(_ => _.FillArea(20, 20, 3, 3, TileType.Mountain)).Should().NotThrow();
	}

	[Fact]
	public void CanPlace_Should_Follow_SurfaceRules()
	{
		var surface = CreateSurface(6, 4);
		surface.SetTileType(0, 0, TileType.Plain);
		surface.SetTileType(1, 0, TileType.Mountain);

		surface.CanPlace(0, 0).Should().BeTrue();
		surface.CanPlace(1, 0).Should().BeFalse();
	}

	[Fact]
	public void CanPlaceArea_Should_Return_False_If_Any_Tile_Is_Blocking()
	{
		var surface = CreateSurface(8, 6);
		surface.FillArea(1, 1, 4, 3, TileType.Plain);
		surface.SetTileType(2, 2, TileType.Mountain);

		surface.CanPlaceArea(1, 1, 4, 3).Should().BeFalse();
		surface.CanPlaceArea(1, 1, 1, 1).Should().BeTrue();
	}

	[Fact]
	public void CanPlaceArea_Should_Clip_And_Return_False_For_Empty_Rect()
	{
		var surface = CreateSurface(5, 5);
		surface.CanPlaceArea(10, 10, 3, 3).Should().BeFalse();
		surface.FillArea(0, 0, 5, 5, TileType.Plain);
		surface.CanPlaceArea(3, 3, 5, 5).Should().BeTrue();
		surface.SetTileType(4, 4, TileType.Mountain);
		surface.CanPlaceArea(3, 3, 5, 5).Should().BeFalse();
	}

	[Fact]
	public void GetTerritoryInfo_Should_Return_Null_For_Unknown_Id()
	{
		var surface = CreateSurface(6, 6, 2, 2);
		surface.GetTerritoryInfo(0).Should().BeNull();
	}
}
