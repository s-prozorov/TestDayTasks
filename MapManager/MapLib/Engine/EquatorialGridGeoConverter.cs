using MapLib.Domain;

namespace MapLib.Engine;

/// <summary>
/// Конвертер координат между тайловой картой (x, y в тайлах)
/// и географическими координатами (долгота, широта).
/// Использует экваториальное приближение: 1° ≈ 111 км.
/// </summary>
public class EquatorialGridGeoConverter : IGridGeoConverter
{
	private const double M_PER_DEG_LAT = 110574d;
	private const double M_PER_DEG_LON = 111320d;
	private readonly double _s;
	private readonly double _lon0;
	private readonly double _lat0;

	/// <summary>
	/// Создает конвертер для заданного масштаба тайла.
	/// </summary>
	public EquatorialGridGeoConverter(double tileSizeMeters = 10, double lon0 = 0, double lat0 = 0)
	{
		_s = tileSizeMeters > 0 ? tileSizeMeters : throw new ArgumentOutOfRangeException(nameof(tileSizeMeters));
		_lon0 = lon0;
		_lat0 = lat0;
	}

	/// <summary>
	/// Преобразует координаты сетки в географические координаты.
	/// </summary>
	public (double lon, double lat) ToGeo(double xTiles, double yTiles) =>
		(
			_lon0 + (xTiles * _s) / M_PER_DEG_LON, 
			_lat0 + (yTiles * _s) / M_PER_DEG_LAT
		);

	/// <summary>
	/// Преобразует географические координаты в координаты сетки.
	/// </summary>
	public (double xTiles, double yTiles) ToGrid(double lon, double lat) =>
		(
			((lon - _lon0) * M_PER_DEG_LON) / _s,
			((lat - _lat0) * M_PER_DEG_LAT) / _s
		);

	/// <summary>
	/// Преобразует количество тайлов в метры, исходя из текущего масштаба.
	/// </summary>
	public double TilesToMeters(double tiles) => tiles * _s;
}