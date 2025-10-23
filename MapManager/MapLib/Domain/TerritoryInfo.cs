namespace MapLib.Domain;

/// <summary>
/// Информация о территории на карте.
/// </summary>
public class TerritoryInfo
{
	/// <summary>
	/// Уникальный идентификатор территории.
	/// Совпадает с идентификатором, записанным в старших битах тайла.
	/// </summary>
    public int Id { get; set; }
}
