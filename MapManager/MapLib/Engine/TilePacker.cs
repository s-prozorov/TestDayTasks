using MapLib.Domain;

namespace MapLib.Engine;

/// <summary>
/// Служебный класс для упаковки и распаковки тайлов карты.
/// </summary>
internal static class TilePacker
{
	/// <summary>
	/// Упаковывает идентификатор региона и тип тайла в одно значение.
	/// </summary>
	internal static long Pack(int regionId, TileType type) => 
		((long)regionId << 32) | (uint)type;

	/// <summary>
	/// Извлекает идентификатор региона.
	/// </summary>
	/// <param name="tile"></param>
	/// <returns></returns>
	internal static int UnpackRegionId(long tile) =>
		(int)(tile >> 32);

	/// <summary>
	/// Извлекает тип тайла.
	/// </summary>
	internal static TileType UnpackType(long tile) =>
		(TileType)(tile & 0xFFFFFFFF);
}