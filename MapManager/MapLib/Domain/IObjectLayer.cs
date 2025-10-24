namespace MapLib.Domain;

/// <summary>
/// Интерфейс слоя объектов.
/// </summary>
public interface IObjectLayer
{
	/// <summary>
	/// Добавляет новый объект на карту.
	/// </summary>
	void Add(MapObject mapObject);

	/// <summary>
	/// Перемещает существующий объект в новые координаты.
	/// </summary>
	void Move(ObjectId id, int newX, int newY);

	/// <summary>
	/// Изменяет размеры объекта.
	/// </summary>
	void Resize(ObjectId id, int newWidth, int newHeight);

	/// <summary>
	/// Удаляет объект с карты.
	/// </summary>
	void Remove(ObjectId id);

	/// <summary>
	/// Возвращает объект по идентификатору.
	/// </summary>
	MapObject? GetById(ObjectId id);

	/// <summary>
	/// Возвращает объект, находящийся в указанных координатах.
	/// </summary>
	MapObject? GetAt(int x, int y);

	/// <summary>
	/// Возвращает все объекты, пересекающие указанную область.
	/// </summary>
	IReadOnlyList<MapObject> GetInArea(int x, int y, int width, int height);

	/// <summary>
	/// Проверяет, пересекается ли указанный объект с областью.
	/// </summary>
	bool Intersects(ObjectId id, int x, int y, int width, int height);

	/// <summary>
	/// Событие при создании нового объекта.
	/// </summary>
	event Action<MapObject> Created;

	/// <summary>
	/// Событие при изменении объекта.
	/// </summary>
	event Action<MapObject> Updated;

	/// <summary>
	/// Событие при удалении объекта.
	/// </summary>
	event Action<ObjectId> Deleted;
}