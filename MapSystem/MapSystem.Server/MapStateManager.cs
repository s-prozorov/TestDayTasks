using System.Collections.Concurrent;
using MapSystem.Contracts;
using MapSystem.Contracts.Events;
using MapSystem.Core.ObjectLayer;
using MapSystem.Core.RegionLayer;

namespace MapSystem.Server;

public sealed class MapStateManager : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ConcurrentBag<Action<ObjectAddedEvent>> _objectAddedHandlers = new();
    private readonly ConcurrentBag<Action<ObjectDeletedEvent>> _objectDeletedHandlers = new();
    private readonly IObjectLayer _objectLayer;
    private readonly ConcurrentBag<Action<ObjectUpdatedEvent>> _objectUpdatedHandlers = new();
    private readonly IRegionLayer _regionLayer;
    private bool _disposed;

    public MapStateManager(IObjectLayer objectLayer, IRegionLayer regionLayer)
    {
        _objectLayer = objectLayer ?? throw new ArgumentNullException(nameof(objectLayer));
        _regionLayer = regionLayer ?? throw new ArgumentNullException(nameof(regionLayer));

        _objectLayer.ObjectAdded += OnObjectLayerAdded;
        _objectLayer.ObjectUpdated += OnObjectLayerUpdated;
        _objectLayer.ObjectDeleted += OnObjectLayerDeleted;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _objectLayer.ObjectAdded -= OnObjectLayerAdded;
        _objectLayer.ObjectUpdated -= OnObjectLayerUpdated;
        _objectLayer.ObjectDeleted -= OnObjectLayerDeleted;

        _lock.Dispose();
        _disposed = true;
    }

    public async Task<ObjectData[]> GetObjectsInAreaAsync(int x, int y, int width, int height)
    {
        _lock.EnterReadLock();
        try
        {
            var objects = await _objectLayer.GetInAreaAsync(x, y, width, height);
            return objects.Select(MapToObjectData).ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public RegionData[] GetRegionsInArea(int x, int y, int width, int height)
    {
        _lock.EnterReadLock();
        try
        {
            var regions = _regionLayer.GetRegionsInArea(x, y, width, height);
            return regions.Select(MapToRegionData).ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void SubscribeObjectAdded(Action<ObjectAddedEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _objectAddedHandlers.Add(handler);
    }

    public void SubscribeObjectUpdated(Action<ObjectUpdatedEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _objectUpdatedHandlers.Add(handler);
    }

    public void SubscribeObjectDeleted(Action<ObjectDeletedEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _objectDeletedHandlers.Add(handler);
    }

    private void OnObjectLayerAdded(MapObject obj)
    {
        var evt = new ObjectAddedEvent { Object = MapToObjectData(obj) };
        foreach (var handler in _objectAddedHandlers)
            try
            {
                handler(evt);
            }
            catch
            {
            }
    }

    private void OnObjectLayerUpdated(MapObject obj)
    {
        var evt = new ObjectUpdatedEvent { Object = MapToObjectData(obj) };
        foreach (var handler in _objectUpdatedHandlers)
            try
            {
                handler(evt);
            }
            catch
            {
                // Log but don't crash
            }
    }

    private void OnObjectLayerDeleted(uint objectId)
    {
        var evt = new ObjectDeletedEvent { ObjectId = objectId };
        foreach (var handler in _objectDeletedHandlers)
            try
            {
                handler(evt);
            }
            catch
            {
                // Log but don't crash
            }
    }

    private static ObjectData MapToObjectData(MapObject obj)
    {
        return new ObjectData
        {
            Id = obj.Id,
            X = obj.X,
            Y = obj.Y,
            Width = obj.Width,
            Height = obj.Height
        };
    }

    private static RegionData MapToRegionData(Region region)
    {
        return new RegionData
        {
            Id = region.Id,
            Name = region.Name
        };
    }
}