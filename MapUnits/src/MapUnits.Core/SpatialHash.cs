using System.Collections.Concurrent;

namespace MapUnits.Core;

internal class SpatialHash<T> where T : class
{
    private const int DEFAULT_CELL_SIZE = 32;
    
    private readonly ConcurrentDictionary<long, List<T>> _cells;
    private readonly int _cellSize;
    private readonly Func<T, (int x, int y)> _getPosition;

    public SpatialHash(int cellSize, Func<T, (int x, int y)> getPosition)
    {
        _cellSize = cellSize > 0 ? cellSize : DEFAULT_CELL_SIZE;
        _getPosition = getPosition;
        _cells = new ConcurrentDictionary<long, List<T>>();
    }

    public void Add(T item)
    {
        var pos = _getPosition(item);
        long key = GetKey(pos.x / _cellSize, pos.y / _cellSize);
        
        _cells.AddOrUpdate(key, 
            _ => new List<T> { item }, 
            (_, list) => { 
                lock(list) { 
                    list.Add(item); 
                } 
                return list; 
            });
    }

    public void Remove(T item)
    {
        var (x, y) = _getPosition(item);
        long key = GetKey(x / _cellSize, y / _cellSize);
        
        if (_cells.TryGetValue(key, out var list))
            lock(list) { list.Remove(item); }
    }

    public List<T> Query(int x, int y, int width, int height)
    {
        int startCellX = x / _cellSize;
        int startCellY = y / _cellSize;
        int endCellX = (x + width - 1) / _cellSize;
        int endCellY = (y + height - 1) / _cellSize;
        
        int estimatedCells = (endCellX - startCellX + 1) * (endCellY - startCellY + 1);
        var results = new List<T>(estimatedCells * 16);

        for (int cy = startCellY; cy <= endCellY; ++cy)
        {
            for (int cx = startCellX; cx <= endCellX; ++cx)
            {
                long key = GetKey(cx, cy);
                if (_cells.TryGetValue(key, out List<T>? list))
                {
                    lock(list) { 
                        results.AddRange(list); 
                    }
                }
            }
        }

        return results;
    }

    public void Clear()
    {
        _cells.Clear();
    }

    private static long GetKey(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }
}

