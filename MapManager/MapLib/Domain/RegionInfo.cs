namespace MapLib.Domain;

/// <summary>
/// Информация о территории на карте.
/// </summary>
public class RegionInfo
{
	/// <summary>
	/// Уникальный идентификатор территории.
	/// Совпадает с идентификатором, записанным в старших битах тайла.
	/// </summary>
    public int Id { get; set; }

	/// <summary>
	/// Название региона.
	/// </summary>
	public string Name { get; set; } = string.Empty;
}
