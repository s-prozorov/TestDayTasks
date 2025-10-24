using MapLib.Domain;
using StackExchange.Redis;

namespace MapLib.Engine;

/// <summary>
/// Слой объектов на Redis.
/// </summary>
public class RedisObjectLayer : IObjectLayer
{
	private readonly IDatabase _database;
	private readonly IGridGeoConverter _converter;
	private readonly string _geoKey;
	private readonly string _objKeyPrefix;

	/// <summary>
	/// Событие создания объекта.
	/// </summary>
	public event Action<MapObject>? Created;

	/// <summary>
	/// Событие изменения объекта.
	/// </summary>
	public event Action<MapObject>? Updated;

	/// <summary>
	/// Событие удаления объекта.
	/// </summary>
	public event Action<ObjectId>? Deleted;

	/// <summary>
	/// Создает экземпляр слоя объектов, хранящего данные в Redis.
	/// </summary>
	public RedisObjectLayer(IConnectionMultiplexer multiplexer, IGridGeoConverter converter,
		string geoKey = "obj:geo", string objKeyPrefix = "obj:")
	{
		_database = multiplexer?.GetDatabase() ?? throw new ArgumentNullException(nameof(multiplexer));
		_converter = converter ?? throw new ArgumentNullException(nameof(converter));
		_geoKey = geoKey;
		_objKeyPrefix = objKeyPrefix;
	}

	/// <summary>
	/// Добавляет объект - сохраняет HASH и индексирует центр объекта в гео-индексе.
	/// </summary>
	/// <param name="mapObject"></param>
	/// <exception cref="InvalidOperationException"></exception>
	public void Add(MapObject mapObject)
	{
		Validate(mapObject);
		var (lon, lat) = CenterLonLat(mapObject);
		var transaction = _database.CreateTransaction();
		_ = transaction.HashSetAsync(BuildObjectKey(mapObject.Id), ToHash(mapObject));
		_ = transaction.GeoAddAsync(_geoKey, lon, lat, mapObject.Id.Value);
		if(!transaction.Execute())
		{
			throw new InvalidOperationException("Add failed");
		}
		Created?.Invoke(mapObject);
	}

	/// <summary>
	/// Перемещает объект в новые координаты.
	/// </summary>
	public void Move(ObjectId id, int newX, int newY) =>
		UpdateInternal(id, o => o with { X = newX, Y = newY });

	/// <summary>
	/// Меняет размеры объекта.
	/// </summary>
	public void Resize(ObjectId id, int newWidth, int newHeight) =>
		UpdateInternal(id, o => o with { Width = newWidth, Height = newHeight });

	/// <summary>
	/// Удаляет объект - HASH и запись из гео-индекса.
	/// </summary>
	/// <param name="id"></param>
	/// <exception cref="InvalidOperationException"></exception>
	public void Remove(ObjectId id)
	{
		var transaction = _database.CreateTransaction();
		_ = transaction.KeyDeleteAsync(BuildObjectKey(id));
		_ = transaction.SortedSetRemoveAsync(_geoKey, id.Value);
		if(!transaction.Execute())
		{
			throw new InvalidOperationException("Remove failed");
		}
		Deleted?.Invoke(id);
	}

	/// <summary>
	/// Возвращает объект по идентификатору.
	/// </summary>
	public MapObject? GetById(ObjectId id) => 
		FromHash(_database.HashGetAll(BuildObjectKey(id)));

	/// <summary>
	/// Возвращает объект, покрывающий тайл.
	/// </summary>
	public MapObject? GetAt(int x, int y) => GetInArea(x, y, 1, 1).FirstOrDefault();

	/// <summary>
	/// Возвращает все объекты, пересекающие прямоугольную область.
	/// </summary>
	public IReadOnlyList<MapObject> GetInArea(int x, int y, int width, int height)
	{
		if(width <= 0 || height <= 0)
		{
			return Array.Empty<MapObject>();
		}

		// конвертация в долготу и широту центра области в тайлах
		var cx = x + width / 2.0;
		var cy = y + height / 2.0;
		var (lon, lat) = _converter.ToGeo(cx, cy);

		// используем окружность с радиусом, равной половине диагонали прямоугольника
		var widthMeters = _converter.TilesToMeters(width);
		var heightMeters = _converter.TilesToMeters(height);
		var radiusResults = _database.GeoSearch(_geoKey, lon, lat, new GeoSearchBox(widthMeters, heightMeters));
		if(radiusResults.Length == 0)
		{
			return Array.Empty<MapObject>();
		}

		// пакетная загрузка и фильтрация - проверка пересечения ограничивающих прямоугольников
		var pipe = _database.CreateBatch();
		var tasks = new List<Task<HashEntry[]>>(radiusResults.Length);
		foreach(var radiusResult in radiusResults)
		{
			tasks.Add(pipe.HashGetAllAsync(BuildObjectKey(new ObjectId(radiusResult.Member))));
		}
		pipe.Execute();
		Task.WaitAll(tasks.ToArray());

		var objects = tasks.Select(t => FromHash(t.Result)).Where(o => o is not null).ToArray();
		return objects.Where(o => IntersectsRect(o, x, y, width, height)).ToArray();
	}

	/// <summary>
	/// Проверяет пересечение объекта по Id с прямоугольной областью
	/// </summary>
	public bool Intersects(ObjectId id, int x, int y, int width, int height)
	{
		var obj = GetById(id);
		return obj is not null && IntersectsRect(obj, x, y, width, height);
	}

	/// <summary>
	/// Формирует полный ключ Redis для хранения данных объекта (HASH),
	/// </summary>
	private string BuildObjectKey(ObjectId id) => _objKeyPrefix + id.Value;

	/// <summary>
	/// Проверяет корректность данных объекта перед сохранением.
	/// </summary>
	private static void Validate(MapObject mapObject)
	{
		if(string.IsNullOrWhiteSpace(mapObject.Id.Value))
		{
			throw new ArgumentException("Id required");
		}

		if(mapObject.Width <= 0 || mapObject.Height <= 0)
		{
			throw new ArgumentOutOfRangeException("Width/Height must be > 0");
		}
	}

	/// <summary>
	/// Вычисляет долготу и широту центра объекта для индексации
	/// </summary>
	private (double lon, double lat) CenterLonLat(MapObject mapObject) =>
		_converter.ToGeo(mapObject.X + mapObject.Width / 2.0, mapObject.Y + mapObject.Height / 2.0);

	/// <summary>
	/// Обновление объекта - чтение текущего объекта, применение изменений, валидация,
	/// обновление HASH и гео-индекса, отправка события.
	/// </summary>
	private void UpdateInternal(ObjectId id, Func<MapObject, MapObject> mutate)
	{
		var current = GetById(id) ?? throw new KeyNotFoundException($"Object {id} not found");
		var next = mutate(current);
		Validate(next);
		var (lon, lat) = CenterLonLat(next);
		var transaction = _database.CreateTransaction();
		_ = transaction.HashSetAsync(BuildObjectKey(id), ToHash(next));
		_ = transaction.GeoAddAsync(_geoKey, lon, lat, id.Value);
		if(!transaction.Execute())
		{
			throw new InvalidOperationException("Update failed");
		}
		Updated?.Invoke(next);
	}

	/// <summary>
	/// Сериализует объект в набор полей HASH Redis.
	/// </summary>
	private static HashEntry[] ToHash(MapObject mapObject) =>
		new[]
		{
			new HashEntry("id", mapObject.Id.Value),
			new HashEntry("x", mapObject.X),
			new HashEntry("y", mapObject.Y),
			new HashEntry("width", mapObject.Width),
			new HashEntry("height", mapObject.Height)
		};

	/// <summary>
	/// Десериализует объект из полей HASH Redis.
	/// </summary>
	/// <param name="hash"></param>
	/// <returns></returns>
	private static MapObject? FromHash(HashEntry[]? hash)
	{
		if(hash is null || hash.Length == 0)
		{
			return null;
		}

		var dictionary = hash.ToDictionary(e => (string)e.Name!, e => e.Value);
		return new MapObject
		{
			Id = new ObjectId((string)dictionary["id"]),
			X = (int)dictionary["x"],
			Y = (int)dictionary["y"],
			Width = (int)dictionary["width"],
			Height = (int)dictionary["height"]
		};
	}

	/// <summary>
	/// Проверяет пересечение прямоугольников
	/// </summary>
	/// <returns></returns>
	private static bool IntersectsRect(MapObject mapObject, int x, int y, int width, int height) =>
		mapObject.X < x + width && x < mapObject.X + mapObject.Width && mapObject.Y < y + height && y < mapObject.Y + mapObject.Height;
}