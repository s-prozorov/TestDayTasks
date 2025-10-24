namespace MapLib.Domain;

/// <summary>
/// Интерфейс доступа к карте тайлов.
/// </summary>
public interface ITileMap
{
	/// <summary>
	/// Ширина карты в тайлах.
	/// </summary>
	int Width { get; }

	/// <summary>
	/// Высота карты в тайлах.
	/// </summary>
	int Height { get; }

	/// <summary>
	/// Возвращает тип тайла по указанным координатам.
	/// </summary>
	TileType GetTileType(int x, int y);

	/// <summary>
	/// Проверяет возможность размещения объекта по указанным координатам.
	/// </summary>
	bool CanPlace(int x, int y);
}