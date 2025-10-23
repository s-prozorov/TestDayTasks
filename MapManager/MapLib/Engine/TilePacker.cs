using MapLib.Domain;

namespace MapLib.Engine;

/// <summary>
/// Служебный класс для упаковки и распаковки тайлов карты.
/// </summary>
internal static class TilePacker
{
	/// <summary>
	/// Упаковывает идентификатор территории и тип тайла в одно значение.
	/// </summary>
	internal static long Pack(int territoryId, TileType type) => 
		((long)territoryId << 32) | (uint)type;

	/// <summary>
	/// Извлекает идентификатор территории.
	/// </summary>
	/// <param name="tile"></param>
	/// <returns></returns>
	internal static int UnpackTerritoryId(long tile) =>
		(int)(tile >> 32);

	/// <summary>
	/// Извлекает тип тайла.
	/// </summary>
	internal static TileType UnpackType(long tile) =>
		(TileType)(tile & 0xFFFFFFFF);
}