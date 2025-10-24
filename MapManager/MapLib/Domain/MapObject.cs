namespace MapLib.Domain;

/// <summary>
/// Описание объекта, размещенного на карте.
/// </summary>
public record MapObject
{
	/// <summary>
	/// Уникальный идентификатор объекта.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	/// Координата X верхнего левого угла (в тайлах).
	/// </summary>
	public int X { get; init; }

	/// <summary>
	/// Координата Y верхнего левого угла (в тайлах).
	/// </summary>
	public int Y { get; init; }
	
	/// <summary>
	/// Ширина объекта в тайлах.
	/// </summary>
	public int Width { get; init; }

	/// <summary>
	/// Высота объекта в тайлах.
	/// </summary>
	public int Height { get; init; }
}