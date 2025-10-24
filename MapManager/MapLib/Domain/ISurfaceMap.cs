namespace MapLib.Domain;

/// <summary>
/// Интерфейс слоя поверхности карты.
/// </summary>
public interface ISurfaceMap : ITileMap
{
	/// <summary>
	/// Устанавливает тип поверхности для указанного тайла.
	/// </summary>
	void SetTileType(int x, int y, TileType type);

	/// <summary>
	/// Загружает данные слоя из последовательности упакованных тайлов.
	/// </summary>
	void LoadFrom(ReadOnlySpan<long> tiles);

	/// <summary>
	/// Заполняет прямоугольную область карты указанным типом тайлов.
	/// </summary>
	void FillArea(int x, int y, int width, int height, TileType type);

	/// <summary>
	/// Проверяет возможность размещения объекта на указанной прямоугольной области.
	/// </summary>
	/// <returns></returns>
	bool CanPlaceArea(int x, int y, int width, int height);
}