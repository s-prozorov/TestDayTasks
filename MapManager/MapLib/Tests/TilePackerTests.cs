using FluentAssertions;
using MapLib.Domain;
using MapLib.Engine;
using Xunit;

namespace MapLib.Tests;

/// <summary>
/// Тесты для TilePacker:
/// проверяют корректность упаковки и распаковки идентификатора и типа тайла.
/// </summary>
public class TilePackerTests
{
	[Theory]
	[InlineData(1, TileType.Plain)]
	[InlineData(1, TileType.Mountain)]
	[InlineData(42, TileType.Plain)]
	[InlineData(int.MaxValue, TileType.Mountain)]
	[InlineData(0, TileType.Plain)]
	public void Pack_And_Unpack_Should_Preserve_Data(int territoryId, TileType type)
	{
		var tile = TilePacker.Pack(territoryId, type);

		TilePacker.UnpackTerritoryId(tile).Should().Be(territoryId);
		TilePacker.UnpackType(tile).Should().Be(type);
	}

	[Fact]
	public void Pack_Should_Encode_TerritoryId_In_Upper_32Bits()
	{
		var id = 123;
		var type = TileType.Mountain;

		var tile = TilePacker.Pack(id, type);

		(tile >> 32).Should().Be(id);
	}

	[Fact]
	public void Pack_Should_Encode_TileType_In_Lower_32Bits()
	{
		var id = 123;
		var type = TileType.Mountain;

		var tile = TilePacker.Pack(id, type);

		(tile & 0xFFFFFFFF).Should().Be((int)type);
	}

	[Fact]
	public void UnpackTerritoryId_Should_Extract_UpperBits_Correctly()
	{
		var tile = 0x0000000200000000L;

		var id = TilePacker.UnpackTerritoryId(tile);

		id.Should().Be(2);
	}

	[Fact]
	public void UnpackType_Should_Extract_LowerBits_Correctly()
	{
		var tile = 0x0000000000000001L;

		var type = TilePacker.UnpackType(tile);

		type.Should().Be(TileType.Mountain);
	}

	[Fact]
	public void Pack_Should_Be_Deterministic()
	{
		const int id = 77;
		const TileType type = TileType.Mountain;

		var t1 = TilePacker.Pack(id, type);
		var t2 = TilePacker.Pack(id, type);

		t1.Should().Be(t2);
	}
}
