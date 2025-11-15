using MapSystem.Core.ObjectLayer;
using Testcontainers.Redis;

namespace MapSystem.Core.Tests;

public class ObjectLayerTests : IAsyncLifetime
{
    private RedisObjectLayer _layer = null!;
    private RedisContainer _redisContainer = null!;

    public async Task InitializeAsync()
    {
        _redisContainer = new RedisBuilder().Build();
        await _redisContainer.StartAsync();
        _layer = await RedisObjectLayer.CreateAsync(_redisContainer.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        _layer?.Dispose();
        await _redisContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ValidParameters_ReturnsId()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        Assert.True(id > 0);
    }

    [Fact]
    public async Task AddAsync_MultipleObjects_ReturnsDifferentIds()
    {
        var id1 = await _layer.AddAsync(10, 20, 5, 3);
        var id2 = await _layer.AddAsync(30, 40, 2, 2);

        Assert.NotEqual(id1, id2);
        Assert.True(id2 > id1);
    }

    [Theory]
    [InlineData(-1, 20, 5, 3)]
    [InlineData(10, -1, 5, 3)]
    [InlineData(10, 20, 0, 3)]
    [InlineData(10, 20, 5, 0)]
    [InlineData(10, 20, -1, 3)]
    public async Task AddAsync_InvalidParameters_ThrowsException(int x, int y, int width, int height)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _layer.AddAsync(x, y, width, height));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingObject_ReturnsObject()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var obj = await _layer.GetByIdAsync(id);

        Assert.NotNull(obj);
        Assert.Equal(id, obj.Id);
        Assert.Equal(10, obj.X);
        Assert.Equal(20, obj.Y);
        Assert.Equal(5, obj.Width);
        Assert.Equal(3, obj.Height);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingObject_ReturnsNull()
    {
        var obj = await _layer.GetByIdAsync(99999);

        Assert.Null(obj);
    }

    [Fact]
    public async Task RemoveAsync_ExistingObject_ReturnsTrue()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var removed = await _layer.RemoveAsync(id);

        Assert.True(removed);
    }

    [Fact]
    public async Task RemoveAsync_NonExistingObject_ReturnsFalse()
    {
        var removed = await _layer.RemoveAsync(99999);

        Assert.False(removed);
    }

    [Fact]
    public async Task RemoveAsync_ExistingObject_ObjectNoLongerExists()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);
        await _layer.RemoveAsync(id);

        var obj = await _layer.GetByIdAsync(id);

        Assert.Null(obj);
    }

    [Fact]
    public async Task GetAtPositionAsync_ObjectAtPosition_ReturnsObject()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var obj = await _layer.GetAtPositionAsync(12, 21);

        Assert.NotNull(obj);
        Assert.Equal(id, obj.Id);
    }

    [Fact]
    public async Task GetAtPositionAsync_ObjectAtTopLeft_ReturnsObject()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var obj = await _layer.GetAtPositionAsync(10, 20);

        Assert.NotNull(obj);
        Assert.Equal(id, obj.Id);
    }

    [Fact]
    public async Task GetAtPositionAsync_ObjectAtBottomRight_ReturnsObject()
    {
        await _layer.AddAsync(10, 20, 5, 3);

        var obj = await _layer.GetAtPositionAsync(14, 22);

        Assert.NotNull(obj);
    }

    [Fact]
    public async Task GetAtPositionAsync_NoObjectAtPosition_ReturnsNull()
    {
        await _layer.AddAsync(10, 20, 5, 3);

        var obj = await _layer.GetAtPositionAsync(50, 50);

        Assert.Null(obj);
    }

    [Fact]
    public async Task GetAtPositionAsync_OutsideObjectBounds_ReturnsNull()
    {
        await _layer.AddAsync(10, 20, 5, 3);

        var obj = await _layer.GetAtPositionAsync(15, 22);

        Assert.Null(obj);
    }

    [Theory]
    [InlineData(-1, 20)]
    [InlineData(10, -1)]
    public async Task GetAtPositionAsync_InvalidCoordinates_ThrowsException(int x, int y)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _layer.GetAtPositionAsync(x, y));
    }

    [Fact]
    public async Task GetInAreaAsync_ObjectFullyInside_ReturnsObject()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var objects = await _layer.GetInAreaAsync(5, 15, 20, 20);

        Assert.Single(objects);
        Assert.Equal(id, objects.ElementAt(0).Id);
    }

    [Fact]
    public async Task GetInAreaAsync_ObjectPartiallyInside_ReturnsObject()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var objects = await _layer.GetInAreaAsync(12, 21, 2, 1);

        Assert.Single(objects);
        Assert.Equal(id, objects.ElementAt(0).Id);
    }

    [Fact]
    public async Task GetInAreaAsync_ObjectOutside_ReturnsEmpty()
    {
        await _layer.AddAsync(10, 20, 5, 3);

        var objects = await _layer.GetInAreaAsync(50, 50, 10, 10);

        Assert.Empty(objects);
    }

    [Fact]
    public async Task GetInAreaAsync_MultipleObjects_ReturnsAll()
    {
        var id1 = await _layer.AddAsync(10, 20, 5, 3);
        var id2 = await _layer.AddAsync(20, 30, 5, 3);
        var id3 = await _layer.AddAsync(100, 100, 2, 2);

        var objects = await _layer.GetInAreaAsync(5, 15, 30, 30);

        Assert.Equal(2, objects.Count);
        Assert.Contains(objects, o => o.Id == id1);
        Assert.Contains(objects, o => o.Id == id2);
    }

    [Fact]
    public async Task GetInAreaAsync_NoObjects_ReturnsEmpty()
    {
        var objects = await _layer.GetInAreaAsync(0, 0, 100, 100);

        Assert.Empty(objects);
    }

    [Theory]
    [InlineData(-1, 10, 5, 5)]
    [InlineData(10, -1, 5, 5)]
    [InlineData(10, 10, 0, 5)]
    [InlineData(10, 10, 5, 0)]
    public async Task GetInAreaAsync_InvalidParameters_ThrowsException(int x, int y, int width, int height)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _layer.GetInAreaAsync(x, y, width, height));
    }

    [Fact]
    public async Task UpdateAsync_ExistingObject_UpdatesAndReturnsTrue()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var updated = await _layer.UpdateAsync(id, 15, 25, 7, 4);

        Assert.True(updated);
        var obj = await _layer.GetByIdAsync(id);
        Assert.Equal(15, obj.X);
        Assert.Equal(25, obj.Y);
        Assert.Equal(7, obj.Width);
        Assert.Equal(4, obj.Height);
    }

    [Fact]
    public async Task UpdateAsync_ExistingObject_FiresEvent()
    {
        MapObject? updatedObject = null;
        _layer.ObjectUpdated += obj => updatedObject = obj;
        var id = await _layer.AddAsync(10, 20, 5, 3);

        await _layer.UpdateAsync(id, 15, 25, 7, 4);

        Assert.NotNull(updatedObject);
        Assert.Equal(15, updatedObject.X);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingObject_ReturnsFalse()
    {
        var updated = await _layer.UpdateAsync(99999, 10, 20, 5, 3);

        Assert.False(updated);
    }

    [Fact]
    public async Task UpdateAsync_GetAtPositionAsync_FindsAtNewPosition()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);
        await _layer.UpdateAsync(id, 50, 60, 5, 3);

        var objOld = await _layer.GetAtPositionAsync(10, 20);
        var objNew = await _layer.GetAtPositionAsync(50, 60);

        Assert.Null(objOld);
        Assert.NotNull(objNew);
        Assert.Equal(id, objNew.Id);
    }


    [Theory]
    [InlineData(-1, 20, 5, 3)]
    [InlineData(10, -1, 5, 3)]
    [InlineData(10, 20, 0, 3)]
    [InlineData(10, 20, 5, 0)]
    public async Task UpdateObjectAsync_InvalidParameters_ThrowsException(int x, int y, int width, int height)
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _layer.UpdateAsync(id, x, y, width, height));
    }

    [Fact]
    public async Task IsInAreaAsync_ObjectFullyInside_ReturnsTrue()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var isInside = await _layer.IsInAreaAsync(id, 5, 15, 20, 20);

        Assert.True(isInside);
    }

    [Fact]
    public async Task IsInAreaAsync_ObjectPartiallyInside_ReturnsTrue()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var isInside = await _layer.IsInAreaAsync(id, 12, 21, 2, 1);

        Assert.True(isInside);
    }

    [Fact]
    public async Task IsInAreaAsync_ObjectOutside_ReturnsFalse()
    {
        var id = await _layer.AddAsync(10, 20, 5, 3);

        var isInside = await _layer.IsInAreaAsync(id, 50, 50, 10, 10);

        Assert.False(isInside);
    }

    [Fact]
    public async Task IsInAreaAsync_NonExistingObject_ReturnsFalse()
    {
        var isInside = await _layer.IsInAreaAsync(99999, 0, 0, 100, 100);

        Assert.False(isInside);
    }

    [Fact]
    public async Task ObjectAdded_WhenObjectAdded_EventFired()
    {
        MapObject? addedObject = null;
        _layer.ObjectAdded += obj => addedObject = obj;

        var id = await _layer.AddAsync(10, 20, 5, 3);

        Assert.NotNull(addedObject);
        Assert.Equal(id, addedObject.Id);
        Assert.Equal(10, addedObject.X);
        Assert.Equal(20, addedObject.Y);
    }

    [Fact]
    public async Task ObjectDeleted_WhenObjectDeleted_EventFired()
    {
        uint? deletedId = null;
        _layer.ObjectDeleted += id => deletedId = id;
        var objId = await _layer.AddAsync(10, 20, 5, 3);

        await _layer.RemoveAsync(objId);

        Assert.NotNull(deletedId);
        Assert.Equal(objId, deletedId.Value);
    }

    [Fact]
    public async Task ObjectDeleted_WhenNonExistingObjectDeleted_EventNotFired()
    {
        uint? deletedId = null;
        _layer.ObjectDeleted += id => deletedId = id;

        await _layer.RemoveAsync(99999);

        Assert.Null(deletedId);
    }

    [Fact]
    public async Task GetInAreaAsync_EdgeCase_ObjectTouchingBorder()
    {
        var id = await _layer.AddAsync(10, 10, 5, 5);

        var objects = await _layer.GetInAreaAsync(15, 15, 5, 5);

        Assert.Empty(objects);
    }

    [Fact]
    public async Task GetInAreaAsync_EdgeCase_ObjectExactlyArea()
    {
        var id = await _layer.AddAsync(10, 10, 5, 5);

        var objects = await _layer.GetInAreaAsync(10, 10, 5, 5);

        Assert.Single(objects);
        Assert.Equal(id, objects.ElementAt(0).Id);
    }
}