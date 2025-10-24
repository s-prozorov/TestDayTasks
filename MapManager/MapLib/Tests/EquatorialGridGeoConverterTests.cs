using FluentAssertions;
using MapLib.Engine;
using Xunit;

namespace MapLib.Tests;

/// <summary>
/// Тесты для EquatorialGridGeoConverter:
/// проверяют конвертацию, смещения, масштаб и ожидаемые приращения градусов.
/// </summary>
public class EquatorialGridGeoConverterTests
{
	private const double DegLatPerMeter = 1.0 / 110574d;
	private const double DegLonPerMeter = 1.0 / 111320d;
	private const double EpsTiles = 1e-6;
	private const double EpsDeg = 1e-9;

	[Theory]
	[InlineData(0)]
	[InlineData(-5)]
	public void Constructor_Should_Throw_When_TileSize_NonPositive(double tileSize)
	{
		var action = () => new EquatorialGridGeoConverter(tileSize);
		action.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Theory]
	[InlineData(10)]
	[InlineData(2.5)]
	public void ToGeo_ToGrid_RoundTrip_NoOffset(double tileSize)
	{
		var converter = new EquatorialGridGeoConverter(tileSize);
		var (x, y) = (1234.25, 987.75);

		var (lon, lat) = converter.ToGeo(x, y);
		var (rx, ry) = converter.ToGrid(lon, lat);

		rx.Should().BeApproximately(x, EpsTiles);
		ry.Should().BeApproximately(y, EpsTiles);
	}

	[Fact]
	public void ToGeo_ToGrid_RoundTrip_WithOffset()
	{
		var converter = new EquatorialGridGeoConverter(10, 5.5, -12.25);
		var (x, y) = (321.875, 777.125);

		var (lon, lat) = converter.ToGeo(x, y);
		var (rx, ry) = converter.ToGrid(lon, lat);

		rx.Should().BeApproximately(x, EpsTiles);
		ry.Should().BeApproximately(y, EpsTiles);
	}

	[Theory]
	[InlineData(10, 3.0)]
	[InlineData(2.5, 100)]
	public void TilesToMeters_Should_Be_Linear(double tileSize, double tiles)
	{
		var converter = new EquatorialGridGeoConverter(tileSize);
		converter.TilesToMeters(tiles).Should().BeApproximately(tileSize * tiles, 1e-9);
	}

	[Theory]
	[InlineData(10)]
	[InlineData(40)]
	public void ToGeo_MoveOneTile_Shifts_By_Expected_Degrees(double tileSize)
	{
		var converter = new EquatorialGridGeoConverter(tileSize);

		var (lon0, lat0) = converter.ToGeo(0, 0);
		var (lon1, lat1) = converter.ToGeo(1, 1);

		var expectedLonDelta = tileSize * DegLonPerMeter;
		var expectedLatDelta = tileSize * DegLatPerMeter;

		(lon1 - lon0).Should().BeApproximately(expectedLonDelta, EpsDeg);
		(lat1 - lat0).Should().BeApproximately(expectedLatDelta, EpsDeg);
	}
}
