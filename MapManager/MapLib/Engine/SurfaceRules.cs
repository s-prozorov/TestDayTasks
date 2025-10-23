using MapLib.Domain;

namespace MapLib.Engine;

/// <summary>
/// Набор правил для определения свойств поверхности
/// </summary>
internal static class SurfaceRules
{
	/// <summary>
	/// Проверяет возможность размещения на тайле указанного типа
	/// </summary>
	public static bool CanPlace(TileType type) => type switch
	{
		TileType.Plain => true,
		TileType.Mountain => false,
		_ => false
	};
}