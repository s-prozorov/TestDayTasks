using System.Diagnostics;
using MapLib.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MapLib.Engine;

/// <summary>
/// Реализация ISurfaceMap, хранящая карту в виде упакованного массива тайлов.
/// </summary>
public class SurfaceMap : ISurfaceMap
{
	/// <summary>
	/// Ширина карты в тайлах.
	/// </summary>
	public int Width { get; }
	
	/// <summary>
	/// Высота карты в тайлах.
	/// </summary>
	public int Height { get; }

	private readonly long[] _map;
	private readonly Dictionary<int, TerritoryInfo> _territories = new();
	private readonly ILogger<SurfaceMap> _logger;

	/// <summary>
	/// Создает экземпляр SurfaceMap и выполняет генерацию карты.
	/// </summary>
	public SurfaceMap(int width, int height, IMapGenerator generator, ILogger<SurfaceMap>? logger = null)
	{
		if(width <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(width));
		}

		if (height <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(height));
		}

		Width = width;
		Height = height;
		_map = new long[Width * Height];
		_logger = logger ?? NullLogger<SurfaceMap>.Instance;

		var sw = Stopwatch.StartNew();
		generator.Generate(_map, Width, Height, _territories);
		sw.Stop();

		var (plains, mountains) = CountTypes(_map);
		_logger.LogInformation("Map generated: {Width}x{Height} in {Ms} ms. Territories: {Territories}. Types: plains={Plains}%, mountains={Mountains}%",
			Width, Height, sw.ElapsedMilliseconds, _territories.Count, Percent(plains), Percent(mountains));
	}

	/// <summary>
	/// Возвращает тип поверхности тайла по координатам.
	/// </summary>
	public TileType GetTileType(int x, int y) =>
		TilePacker.UnpackType(ReadTile(x, y));

	/// <summary>
	/// Возвращает идентификатор территории, к которой относится тайл.
	/// </summary>
	public int GetTerritoryId(int x, int y) => 
		TilePacker.UnpackTerritoryId(ReadTile(x, y));

	/// <summary>
	/// Возвращает информацию о территории по ее идентификатору.
	/// </summary>
	public TerritoryInfo? GetTerritoryInfo(int id)
	{
		if(_territories.TryGetValue(id, out var info))
			return info;

		_logger.LogWarning("Territory info not found for id={id}", id);
		return null;
	}

	/// <summary>
	/// Проверяет возможность размещения объекта на указанном тайле.
	/// </summary>
	public bool CanPlace(int x, int y) => 
		SurfaceRules.CanPlace(GetTileType(x, y));

	/// <summary>
	/// Устанавливает тип поверхности для указанного тайла, сохраняя идентификатор территории.
	/// </summary>
	public void SetTileType(int x, int y, TileType type)
	{
		var raw = ReadTile(x, y);
		var territoryId = TilePacker.UnpackTerritoryId(raw);
		WriteTile(x, y, TilePacker.Pack(territoryId, type));
	}

	/// <summary>
	/// Загружает данные карты из последовательности упакованных тайлов.
	/// </summary>
	public void LoadFrom(ReadOnlySpan<long> tiles)
	{
		if(tiles.Length != _map.Length)
		{
			throw new ArgumentException("Input length must equal map area.", nameof(tiles));
		}

		tiles.CopyTo(_map.AsSpan());
	}

	/// <summary>
	/// Заполняет прямоугольную область карты указанным типом поверхности.
	/// </summary>
	public void FillArea(int x, int y, int width, int height, TileType type)
	{
		var x0 = Math.Max(0, x);
		var y0 = Math.Max(0, y);
		var x1 = Math.Min(Width, x + Math.Max(0, width));
		var y1 = Math.Min(Height, y + Math.Max(0, height));

		if(x0 >= x1 || y0 >= y1)
		{
			return;
		}

		for(var tileY = y0; tileY < y1; tileY++)
		{
			var rowStart = tileY * Width + x0;
			var rowEnd = tileY * Width + x1;

			for(var idx = rowStart; idx < rowEnd; idx++)
			{
				var raw = _map[idx];
				var territoryId = TilePacker.UnpackTerritoryId(raw);
				_map[idx] = TilePacker.Pack(territoryId, type);
			}
		}
	}

	/// <summary>
	/// Проверяет возможность размещения объекта на прямоугольной области карты.
	/// </summary>
	public bool CanPlaceArea(int x, int y, int width, int height)
	{
		var x0 = Math.Max(0, x);
		var y0 = Math.Max(0, y);
		var x1 = Math.Min(Width, x + Math.Max(0, width));
		var y1 = Math.Min(Height, y + Math.Max(0, height));

		if (x0 >= x1 || y0 >= y1)
		{
			return false;
		}

		for (var tileY = y0; tileY < y1; tileY++)
		{
			var rowStart = tileY * Width + x0;
			var rowEnd = tileY * Width + x1;

			for (var idx = rowStart; idx < rowEnd; idx++)
			{
				var type = TilePacker.UnpackType(_map[idx]);

				if(!SurfaceRules.CanPlace(type))
				{
					return false;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Возвращает упакованное значение тайла по координатам.
	/// </summary>
	private long ReadTile(int x, int y)
	{
		ValidateRange(x, y);
		return _map[Index(x, y)];
	}

	/// <summary>
	/// Записывает упакованное значение тайла по координатам.
	/// </summary>
	private void WriteTile(int x, int y, long tile)
	{
		ValidateRange(x, y);
		_map[Index(x, y)] = tile;
	}

	/// <summary>
	/// Вычисляет индекс элемента в линейном массиве карты по координатам тайла.
	/// </summary>
	private int Index(int x, int y) => y * Width + x;

	/// <summary>
	/// Проверяет, что координаты находятся в пределах карты.
	/// </summary>
	private void ValidateRange(int x, int y)
	{
		if((uint)x >= (uint)Width)
		{
			throw new ArgumentOutOfRangeException(nameof(x));
		}

		if((uint)y >= (uint)Height)
		{
			throw new ArgumentOutOfRangeException(nameof(y));
		}
	}

	/// <summary>
	/// Вычисляет процентное соотношение указанного количества элементов
	/// относительно общего числа тайлов на карте.
	/// </summary>
	private int Percent(int count) => (int)(count * 100L / ((long)Width * Height));

	/// <summary>
	/// Подсчитывает количество тайлов каждого типа на карте.
	/// </summary>
	private (int plains, int mountains) CountTypes(long[] map)
	{
		var plains = 0;
		var mountains = 0;

		foreach(var t in map)
		{
			var type = TilePacker.UnpackType(t);
			switch(type)
			{
				case TileType.Plain:
					plains++;
					break;
				case TileType.Mountain:
					mountains++;
					break;
			}
		}

		return (plains, mountains);
	}
}