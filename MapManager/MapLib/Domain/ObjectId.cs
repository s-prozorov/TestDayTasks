namespace MapLib.Domain;

/// <summary>
/// Уникальный идентификатор объекта.
/// </summary>
public readonly record struct ObjectId
{
	/// <summary>
	/// Строковое значение идентификатора.
	/// </summary>
	public string Value { get; init; }

	/// <summary>
	/// Создает идентификатор по строковому значению.
	/// </summary>
	public ObjectId(string value) : this()
	{
		Value = value;
	}

	/// <summary>
	/// Возвращает строковое представление идентификатора.
	/// </summary>
	public override string ToString() => Value;
}