using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK_1_1.Layers;
using TASK_1_1.Tests.Utilities;

namespace TASK_1_1.Tests;
public class ObjectLayerTests
{
    private ObjectLayer CreateLayerWithFakeRedis(out FakeRedisGeoClient redis)
    {
        redis = new FakeRedisGeoClient();
        return new ObjectLayer(redis);
    }

    [Fact]
    public void AddObject_ShouldStoreInMemoryAndRedis_AndRaiseEvent()
    {
        var layer = CreateLayerWithFakeRedis(out var redis);
        MapObject? addedEventObj = null;
        layer.ObjectAdded += o => addedEventObj = o;

        var obj = new MapObject("obj1", 10, 20, 2, 2);
        layer.AddObject(obj);

        Assert.Equal(obj, addedEventObj);

        var retrieved = layer.GetObjectById("obj1");
        Assert.Equal(obj, retrieved);

        var (lat, lon) = CoordinateConverter.ToGeo(10, 20);
        var redisIds = redis.GeoRadius("map_objects", lon, lat, 0.001);
        Assert.Contains("obj1", redisIds);
    }

    [Fact]
    public void RemoveObject_ShouldRemoveFromMemoryAndRedis_AndRaiseEvent()
    {
        var layer = CreateLayerWithFakeRedis(out var redis);
        MapObject? removedEventObj = null;
        layer.ObjectRemoved += o => removedEventObj = o;

        var obj = new MapObject("obj1", 10, 20, 2, 2);
        layer.AddObject(obj);

        layer.RemoveObject("obj1");

        Assert.Equal(obj, removedEventObj);

        Assert.Null(layer.GetObjectById("obj1"));

        var (lat, lon) = CoordinateConverter.ToGeo(10, 20);
        var redisIds = redis.GeoRadius("map_objects", lon, lat, 0.001);
        Assert.DoesNotContain("obj1", redisIds);
    }

    [Fact]
    public void GetObjectAt_ShouldReturnObject_WhenCoordinateInside()
    {
        var layer = CreateLayerWithFakeRedis(out _);
        var obj = new MapObject("obj1", 5, 5, 3, 3);
        layer.AddObject(obj);

        var inside = layer.GetObjectAt(6, 6);
        Assert.Equal(obj, inside);

        var outside = layer.GetObjectAt(0, 0);
        Assert.Null(outside);
    }

    [Fact]
    public void GetObjectsInArea_ShouldReturnCorrectObjects()
    {
        var layer = CreateLayerWithFakeRedis(out _);
        var obj1 = new MapObject("obj1", 0, 0, 2, 2);
        var obj2 = new MapObject("obj2", 3, 3, 2, 2);
        var obj3 = new MapObject("obj3", 5, 5, 2, 2);

        layer.AddObject(obj1);
        layer.AddObject(obj2);
        layer.AddObject(obj3);

        var objects = layer.GetObjectsInArea(1, 1, 4, 4).ToList();

        Assert.Contains(obj1, objects);
        Assert.Contains(obj2, objects);
        Assert.DoesNotContain(obj3, objects);
    }

    [Fact]
    public void UpdateObject_ShouldModifyObjectAndRaiseEvent()
    {
        var layer = CreateLayerWithFakeRedis(out var redis);
        MapObject? updatedEventObj = null;
        layer.ObjectUpdated += o => updatedEventObj = o;

        var obj = new MapObject("obj1", 10, 20, 2, 2);
        layer.AddObject(obj);

        var updatedObj = new MapObject("obj1", 15, 25, 2, 2);
        layer.UpdateObject(updatedObj);

        Assert.Equal(updatedObj, updatedEventObj);

        var retrieved = layer.GetObjectById("obj1");
        Assert.Equal(updatedObj.X, retrieved!.X);
        Assert.Equal(updatedObj.Y, retrieved.Y);

        var (lat, lon) = CoordinateConverter.ToGeo(15, 25);
        var redisIds = redis.GeoRadius("map_objects", lon, lat, 0.001);
        Assert.Contains("obj1", redisIds);
    }
}
