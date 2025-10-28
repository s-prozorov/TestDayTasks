using System.Collections.Concurrent;
using MapSurface;

namespace MapUnits.Core;

public class UnitMovementService : IUnitMovementService, IDisposable
{
    private readonly ConcurrentDictionary<string, Unit> _units;
    private readonly ConcurrentDictionary<string, Movement> _activeMovements;
    private readonly SpatialHash<Unit> _spatialHash;
    private readonly ISurfaceLayer? _surfaceLayer;
    private readonly Random _rng;
    private readonly Timer _updateTimer;
    private readonly object _updateLock = new();

    public UnitMovementService(int updateIntervalMs = 100, int spatialCellSize = 32, ISurfaceLayer? surfaceLayer = null)
    {
        _units = new ConcurrentDictionary<string, Unit>();
        _activeMovements = new ConcurrentDictionary<string, Movement>();
        _spatialHash = new SpatialHash<Unit>(spatialCellSize, unit => unit.GetGridCell());
        _surfaceLayer = surfaceLayer;
        _rng = new Random();
        _updateTimer = new Timer(UpdateMovements, null, updateIntervalMs, updateIntervalMs);
    }

    public void AddUnit(Unit unit)
    {
        if (_units.TryAdd(unit.Id, unit))
            _spatialHash.Add(unit);
    }

    public Unit? GetUnit(string unitId)
    {
        if (_units.TryGetValue(unitId, out var unit))
            return unit;
        return null;
    }

    public void StartMovement(string unitId, float targetX, float targetY)
    {
        if (!_units.TryGetValue(unitId, out var unit))
            throw new ArgumentException($"Unit {unitId} not found");

        int tx = (int)targetX;
        int ty = (int)targetY;
        
        if (_surfaceLayer != null)
        {
            if (tx < 0 || tx >= _surfaceLayer.Width || ty < 0 || ty >= _surfaceLayer.Height)
                throw new ArgumentException("Target position is out of map bounds");
            
            if (!_surfaceLayer.CanPlaceObjectInArea(tx, ty, 1, 1))
                throw new ArgumentException("Target position is not walkable");
        }

        var speedCoeff = 0.75f + (float)_rng.NextDouble() * 0.5f;
        
        var movement = new Movement(
            unitId,
            unit.X, unit.Y,
            targetX, targetY,
            unit.BaseSpeed * speedCoeff,
            DateTime.UtcNow
        );

        _activeMovements[unitId] = movement;
    }

    public List<Unit> GetUnitsInArea(int x, int y, int width, int height)
    {
        var candidates = _spatialHash.Query(x, y, width, height);
        List<Unit> result = new(candidates.Count);

        foreach (var unit in candidates)
        {
            int ux = (int)unit.X;
            int uy = (int)unit.Y;
            if (ux >= x && ux < x + width && uy >= y && uy < y + height)
                result.Add(unit);
        }

        return result;
    }

    public List<Movement> GetMovementsInArea(int x, int y, int width, int height)
    {
        var result = new List<Movement>(_activeMovements.Count);

        foreach (var movement in _activeMovements.Values)
        {
            if (movement.IntersectsRect(x, y, width, height))
                result.Add(movement);
        }

        return result;
    }

    private void UpdateMovements(object? state)
    {
        if (!Monitor.TryEnter(_updateLock)) return;

        try
        {
            DateTime now = DateTime.UtcNow;
            var completed = new List<string>(_activeMovements.Count / 10);

            foreach (var kvp in _activeMovements)
            {
                Movement movement = kvp.Value;
                
                if (movement.IsCompleted(now))
                {
                    completed.Add(kvp.Key);
                    
                    if (_units.TryGetValue(kvp.Key, out var unit))
                    {
                        _spatialHash.Remove(unit);
                        unit.X = movement.TargetX;
                        unit.Y = movement.TargetY;
                        _spatialHash.Add(unit);
                    }
                }
                else
                {
                    if (!_units.TryGetValue(kvp.Key, out var unit)) continue;
                    
                    var pos = movement.GetCurrentPosition(now);
                    var oldCell = unit.GetGridCell();
                    
                    unit.X = pos.x;
                    unit.Y = pos.y;
                    
                    if (unit.GetGridCell() != oldCell) {
                        _spatialHash.Remove(unit);
                        _spatialHash.Add(unit);
                    }
                }
            }

            foreach (var id in completed)
            {
                _activeMovements.TryRemove(id, out _);
            }
        }
        finally
        {
            Monitor.Exit(_updateLock);
        }
    }

    public bool RemoveUnit(string unitId)
    {
        if (_units.TryRemove(unitId, out var unit))
        {
            _spatialHash.Remove(unit);
            _activeMovements.TryRemove(unitId, out _);
            return true;
        }
        return false;
    }

    public void Dispose() => _updateTimer?.Dispose();
}

