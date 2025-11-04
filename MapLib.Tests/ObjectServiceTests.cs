using MapLib.Core.Models.ObjectModel;
using MapLib.Object.Redis;
using MapLib.Services;
using StackExchange.Redis;
using Xunit;

namespace MapLib.Tests;

public class ObjectServiceTests : IAsyncLifetime
{
    private IConnectionMultiplexer? _redis;
    private ObjectService? _service;

    public async Task InitializeAsync()
    {
        try
        {
            _redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
            var repository = new DataContext(_redis);
            _service = new ObjectService(repository);
            
            var db = _redis.GetDatabase();
            await db.ExecuteAsync("FLUSHDB");
        }
        catch (RedisConnectionException)
        {
            _redis = null;
            _service = null;
        }    }

    public async Task DisposeAsync()
    {
        if (_redis != null)
        {
            await _redis.CloseAsync();
            _redis.Dispose();
        }    
    }
    
    [Fact]
    public async Task CreateObject_ValidData_ReturnsId()
    {
        if (_service == null) return;
        
        var objectId = await _service.CreateObject(500, 500, ObjectType.Base);
        
        Assert.NotEmpty(objectId);
    }
    
    [Fact]
    public async Task DeleteObject_ExistingId_ReturnsTrue()
    {
        if (_service == null) return;
        
        var objectId = await _service.CreateObject(300, 400, ObjectType.Mine);
        
        var result = await _service.DeleteObject(objectId);
        
        Assert.True(result);
    }
    
    [Fact]
    public async Task GetObjectsInZone_ReturnsCorrectObjects()
    {
        if (_service == null) return;

        await _service.CreateObject(500, 500, ObjectType.Temp);
        await _service.CreateObject(500, 500, ObjectType.Temp);
        await _service.CreateObject(500, 500, ObjectType.Temp);
        
        
        var objects = await _service.GetObjects(490, 500, 50);
        
        Assert.True(objects.Count >= 2);
        Assert.Contains(objects, o => o.X == 500 && o.Y == 500);
        Assert.Contains(objects, o => o.X == 500 && o.Y == 500);
    }
    
    [Fact]
    public async Task GetObjectsInZone_ReturnsEmptyListOfObjects()
    {
        if (_service == null) return;

        await _service.CreateObject(500, 500, ObjectType.Temp);
        await _service.CreateObject(500, 500, ObjectType.Temp);
        await _service.CreateObject(500, 500, ObjectType.Temp);
        
        
        var objects = await _service.GetObjects(300, 300, 100);
        
        Assert.Empty(objects);
    }
    
}