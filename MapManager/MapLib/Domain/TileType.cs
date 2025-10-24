namespace MapLib.Domain;

/// <summary>
/// Тип тайла карты.
/// Записывается в младших битах тайла.
/// </summary>
public enum TileType : uint
{
	/// <summary>
	/// Равнина.
	/// </summary>
	Plain = 0,

	/// <summary>
	/// Гора.
	/// </summary>
	Mountain = 1
}