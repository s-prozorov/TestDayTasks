using MapLib.Domain;

namespace MapLib.Engine;

/// <summary>
/// Интерфейс генератора карты.
/// </summary>
public interface IMapGenerator
{
	/// <summary>
	/// Заполняет массив тайлов и словарь территорий.
	/// </summary>
	void Generate(long[] map, int width, int height, Dictionary<int, RegionInfo> regions);
}
