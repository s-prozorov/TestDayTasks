namespace MapLib.Domain;

/// <summary>
/// Интерфейс доступа к карте регионов.
/// </summary>
public interface IRegionMap
{
	/// <summary>
	/// Возвращает идентификатор региона, к которому относится тайл.
	/// </summary>
	int GetRegionId(int x, int y);

	/// <summary>
	/// Возвращает сведения о регионе по его идентификатору.
	/// </summary>
	RegionInfo? GetRegionInfo(int id);

	/// <summary>
	/// Проверяет, принадлежит ли тайл к региону
	/// </summary>
	bool IsInRegion(int x, int y, int regionId);

	/// <summary>
	/// Возвращает все регионы, пересекающие указанную область
	/// </summary>
	IReadOnlyList<int> GetRegionsInArea(int x, int y, int width, int height);
}