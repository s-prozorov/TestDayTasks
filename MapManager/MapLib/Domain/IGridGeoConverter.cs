namespace MapLib.Domain;

/// <summary>
/// Преобразование координат сетки в гео-координаты и обратно.
/// </summary>
public interface IGridGeoConverter
{
	/// <summary>
	/// Преобразует координаты сетки в географические координаты.
	/// </summary>
	(double lon, double lat) ToGeo(double xTiles, double yTiles);

	/// <summary>
	/// Преобразует географические координаты в координаты сетки.
	/// </summary>
	(double xTiles, double yTiles) ToGrid(double lon, double lat);

	/// <summary>
	/// Преобразует количество тайлов в метры
	/// </summary>
	double TilesToMeters(double tiles);
}