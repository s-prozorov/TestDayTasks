using Xunit;
using Testcontainers.Redis;
using MapObjects;

namespace MapObjects.Tests;

public class MapObjectLayerTests : IAsyncLifetime
{
    private RedisContainer _redisContainer = null!;
    private IMapObjectLayer _layer = null!;

    public async Task InitializeAsync()
    {
        _redisContainer = new RedisBuilder().Build();
        await _redisContainer.StartAsync();
        
        var connectionString = _redisContainer.GetConnectionString();
        _layer = new MapObjectLayer(connectionString);
    }

    public async Task DisposeAsync()
    {
        _layer?.Dispose();
        if (_redisContainer != null)
        {
            await _redisContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task AddObjectAsync_ValidObject_AddsSuccessfully()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);

        await _layer.AddObjectAsync(obj);

        var retrieved = await _layer.GetObjectByIdAsync("obj1");
        Assert.NotNull(retrieved);
        Assert.Equal("obj1", retrieved.Id);
        Assert.Equal(10, retrieved.X);
        Assert.Equal(20, retrieved.Y);
        Assert.Equal(5, retrieved.Width);
        Assert.Equal(3, retrieved.Height);
    }

    [Fact]
    public async Task AddObjectAsync_NullObject_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _layer.AddObjectAsync(null!));
    }

    [Fact]
    public async Task GetObjectByIdAsync_NonExistentObject_ReturnsNull()
    {
        var result = await _layer.GetObjectByIdAsync("nonexistent");

        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetObjectByIdAsync_InvalidId_ThrowsArgumentException(string id)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _layer.GetObjectByIdAsync(id));
    }

    [Fact]
    public async Task RemoveObjectAsync_ExistingObject_RemovesSuccessfully()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);
        await _layer.AddObjectAsync(obj);

        var removed = await _layer.RemoveObjectAsync("obj1");

        Assert.True(removed);
        var retrieved = await _layer.GetObjectByIdAsync("obj1");
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task RemoveObjectAsync_NonExistentObject_ReturnsFalse()
    {
        var removed = await _layer.RemoveObjectAsync("nonexistent");

        Assert.False(removed);
    }

    [Fact]
    public async Task GetObjectAtPositionAsync_ObjectExists_ReturnsObject()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);
        await _layer.AddObjectAsync(obj);

        var result = await _layer.GetObjectAtPositionAsync(12, 21);

        Assert.NotNull(result);
        Assert.Equal("obj1", result.Id);
    }

    [Fact]
    public async Task GetObjectAtPositionAsync_NoObjectAtPosition_ReturnsNull()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);
        await _layer.AddObjectAsync(obj);

        var result = await _layer.GetObjectAtPositionAsync(0, 0);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_MultipleObjectsInArea_ReturnsAllObjects()
    {
        var obj1 = new MapObject("obj1", 10, 20, 5, 3);
        var obj2 = new MapObject("obj2", 12, 21, 3, 2);
        var obj3 = new MapObject("obj3", 100, 100, 5, 5);
        
        await _layer.AddObjectAsync(obj1);
        await _layer.AddObjectAsync(obj2);
        await _layer.AddObjectAsync(obj3);

        var results = await _layer.GetObjectsInAreaAsync(10, 20, 10, 10);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, o => o.Id == "obj1");
        Assert.Contains(results, o => o.Id == "obj2");
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_NoObjectsInArea_ReturnsEmptyList()
    {
        var obj = new MapObject("obj1", 100, 100, 5, 5);
        await _layer.AddObjectAsync(obj);

        var results = await _layer.GetObjectsInAreaAsync(0, 0, 10, 10);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetObjectsInAreaAsync_InvalidWidth_ThrowsArgumentException(int width)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _layer.GetObjectsInAreaAsync(0, 0, width, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetObjectsInAreaAsync_InvalidHeight_ThrowsArgumentException(int height)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _layer.GetObjectsInAreaAsync(0, 0, 10, height));
    }

    [Fact]
    public async Task Subscribe_ObjectAdded_NotifiesSubscribers()
    {
        ObjectEvent? receivedEvent = null;
        _layer.Subscribe(evt => receivedEvent = evt);

        var obj = new MapObject("obj1", 10, 20, 5, 3);
        await _layer.AddObjectAsync(obj);

        await Task.Delay(100);
        Assert.NotNull(receivedEvent);
        Assert.Equal(ObjectEventType.Added, receivedEvent.EventType);
        Assert.Equal("obj1", receivedEvent.Object.Id);
    }

    [Fact]
    public async Task Subscribe_ObjectDeleted_NotifiesSubscribers()
    {
        var obj = new MapObject("obj1", 10, 20, 5, 3);
        await _layer.AddObjectAsync(obj);

        ObjectEvent? receivedEvent = null;
        _layer.Subscribe(evt =>
        {
            if (evt.EventType == ObjectEventType.Deleted)
                receivedEvent = evt;
        });

        await _layer.RemoveObjectAsync("obj1");

        await Task.Delay(100);
        Assert.NotNull(receivedEvent);
        Assert.Equal(ObjectEventType.Deleted, receivedEvent.EventType);
        Assert.Equal("obj1", receivedEvent.Object.Id);
    }

    [Fact]
    public async Task Subscribe_MultipleSubscribers_AllReceiveEvents()
    {
        int count1 = 0, count2 = 0;
        _layer.Subscribe(_ => count1++);
        _layer.Subscribe(_ => count2++);

        var obj = new MapObject("obj1", 10, 20, 5, 3);
        await _layer.AddObjectAsync(obj);

        await Task.Delay(100);
        Assert.Equal(1, count1);
        Assert.Equal(1, count2);
    }

    [Fact]
    public void Subscribe_NullHandler_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _layer.Subscribe(null!));
    }

    [Fact]
    public void Unsubscribe_NullHandler_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _layer.Unsubscribe(null!));
    }
}

