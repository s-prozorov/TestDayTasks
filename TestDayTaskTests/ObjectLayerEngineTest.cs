namespace TileMapProjectTests;

using NUnit.Framework;
using Moq;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;
using ObjectLayerEngine;

[TestFixture]
public class ObjectLayerEngineTest
{
    private Mock<IConnectionMultiplexer> _redisMock;
    private Mock<IDatabase> _dbMock;
    private Mock<ISubscriber> _subMock;
    private ObjectLayer _objectLayer;

    [SetUp]
    public void Setup()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _dbMock = new Mock<IDatabase>();
        _subMock = new Mock<ISubscriber>();
        
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_dbMock.Object);
        _redisMock.Setup(r => r.GetSubscriber(It.IsAny<object>())).Returns(_subMock.Object);
        
        _objectLayer = new ObjectLayer(_redisMock.Object);
    }

    [Test]
    public async Task AddOrUpdateObjectAsync_ShouldStoreInRedisAndCache()
    {
        // Arrange
        var obj = new MapObjectDTO { Id = "test", X = 100, Y = 200, Width = 2, Height = 3 };
        _dbMock.Setup(db => db.CreateTransaction(It.IsAny<object>())).Returns(new Mock<ITransaction>().Object);

        // Act
        await _objectLayer.AddOrUpdateObjectAsync(obj);

        // Assert
        _dbMock.Verify(db => db.HashSetAsync($"{obj.Id}{ObjectLayer.ObjectPrefix}", 
            It.IsAny<HashEntry[]>(), CommandFlags.FireAndForget), Times.Once);
        _dbMock.Verify(db => db.GeoAddAsync(ObjectLayer.GeoIndexKey, 
            It.IsAny<GeoEntry>(), CommandFlags.FireAndForget), Times.Once);
        Assert.IsTrue(_objectLayer.GetLocalCache().ContainsKey(obj.Id));
    }

    [Test]
    public async Task GetObjectAsync_ShouldReturnFromCacheFirst()
    {
        // Arrange
        var obj = new MapObjectDTO { Id = "cached", X = 50, Y = 50 };
        _objectLayer.GetLocalCache().TryAdd(obj.Id, obj);

        // Act
        var result = await _objectLayer.GetObjectAsync(obj.Id);

        // Assert
        Assert.AreEqual(obj, result);
        _dbMock.Verify(db => db.HashGetAllAsync(It.IsAny<RedisKey>(), CommandFlags.None), Times.Never);
    }

    [Test]
    public async Task RemoveObjectAsync_ShouldDeleteFromAllStores()
    {
        // Arrange
        var obj = new MapObjectDTO { Id = "toDelete", X = 100, Y = 200, Width = 2, Height = 3  };
        _objectLayer.GetLocalCache().TryAdd(obj.Id, obj);
        var transactionMock = new Mock<ITransaction>();
        _dbMock.Setup(db => db.CreateTransaction(It.IsAny<object>())).Returns(transactionMock.Object);

        // Act
        var result = await _objectLayer.RemoveObjectAsync(obj.Id);

        // Assert
        Assert.IsTrue(result);
        _dbMock.Verify(db => db.KeyDeleteAsync($"{obj.Id}{ObjectLayer.ObjectPrefix}", CommandFlags.FireAndForget), Times.Once);
        _dbMock.Verify(db => db.GeoRemoveAsync(ObjectLayer.GeoIndexKey, obj.Id, CommandFlags.FireAndForget), Times.Once);
        Assert.IsFalse(_objectLayer.GetLocalCache().ContainsKey(obj.Id));
    }

    [Test]
    public async Task FindObjectAt_EdgeCases()
    {
        // Test for 0,0 coordinates
        var (lon, lat) = CoordinateConverter.ConvertToGeo(0, 0);
        _dbMock.Setup(db => db.GeoRadiusAsync(ObjectLayer.GeoIndexKey, lon, lat, 1, GeoUnit.Meters, 1, 
            Order.Ascending, GeoRadiusOptions.WithCoordinates, CommandFlags.None))
            .ReturnsAsync(new GeoRadiusResult[] { new GeoRadiusResult("test", null, 0, new GeoPosition(lon, lat)) });

        var result = await _objectLayer.FindObjectAtAsync(0, 0);
        Assert.IsNotNull(result);
    }

    [Test]
    public async Task GetObjectsInArea_OverlappingObjects()
    {
        var area = new { x1 = 10, y1 = 10, x2 = 20, y2 = 20 };
        var objInside = new MapObjectDTO { Id = "inside", X = 15, Y = 15, Width = 2, Height = 2 };
        var objOverlap = new MapObjectDTO { Id = "overlap", X = 5, Y = 5, Width = 10, Height = 10 };

        // Setup mock for getting object details
        _dbMock.Setup(db => db.HashGetAll(
            It.IsAny<RedisKey>(), 
            CommandFlags.None))
            .Returns((RedisKey key) =>
            {
                if (key.ToString().Contains("inside"))
                    return new HashEntry[]
                    {
                        new HashEntry("x", objInside.X),
                        new HashEntry("y", objInside.Y),
                        new HashEntry("width", objInside.Width),
                        new HashEntry("height", objInside.Height)
                    };
                if (key.ToString().Contains("overlap"))
                    return new HashEntry[]
                    {
                        new HashEntry("x", objOverlap.X),
                        new HashEntry("y", objOverlap.Y),
                        new HashEntry("width", objOverlap.Width),
                        new HashEntry("height", objOverlap.Height)
                    };
                return new HashEntry[0];
            });

        // Act
        var results = await _objectLayer.GetObjectsInAreaAsync(area.x1, area.y1, area.x2, area.y2);

        // Assert
        Assert.AreEqual(2, results.Count());
    }

    [Test]
    public async Task EventSubscription_ShouldTriggerOnChanges()
    {
        // Arrange
        var eventMessages = new List<string>();
        _objectLayer.SubscribeToEvents(msg => eventMessages.Add(msg));

        // Act
        await _objectLayer.AddOrUpdateObjectAsync(new MapObjectDTO { Id = "eventTest" });
        await _objectLayer.RemoveObjectAsync("eventTest");

        // Assert
        Assert.Contains("updated:eventTest", eventMessages);
        Assert.Contains("removed:eventTest", eventMessages);
    }

}