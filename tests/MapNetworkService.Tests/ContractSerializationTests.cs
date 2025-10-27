using Xunit;
using MemoryPack;
using MapNetworkService.Contracts;

namespace MapNetworkService.Tests;

public class ContractSerializationTests
{
    [Fact]
    public void MapObjectDto_Serialization_WorksCorrectly()
    {
        var original = new MapObjectDto
        {
            Id = "obj1",
            X = 10,
            Y = 20,
            Width = 5,
            Height = 3
        };

        var serialized = MemoryPackSerializer.Serialize(original);
        var deserialized = MemoryPackSerializer.Deserialize<MapObjectDto>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized!.Id);
        Assert.Equal(original.X, deserialized.X);
        Assert.Equal(original.Y, deserialized.Y);
        Assert.Equal(original.Width, deserialized.Width);
        Assert.Equal(original.Height, deserialized.Height);
    }

    [Fact]
    public void RegionDto_Serialization_WorksCorrectly()
    {
        var original = new RegionDto
        {
            Id = 5,
            Name = "Test Region"
        };

        var serialized = MemoryPackSerializer.Serialize(original);
        var deserialized = MemoryPackSerializer.Deserialize<RegionDto>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized!.Id);
        Assert.Equal(original.Name, deserialized.Name);
    }

    [Fact]
    public void GetObjectsInAreaRequest_Serialization_WorksCorrectly()
    {
        var original = new GetObjectsInAreaRequest
        {
            X = 10,
            Y = 20,
            Width = 100,
            Height = 50
        };

        var serialized = MemoryPackSerializer.Serialize(original);
        var deserialized = MemoryPackSerializer.Deserialize<GetObjectsInAreaRequest>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(original.X, deserialized!.X);
        Assert.Equal(original.Y, deserialized.Y);
        Assert.Equal(original.Width, deserialized.Width);
        Assert.Equal(original.Height, deserialized.Height);
    }

    [Fact]
    public void GetObjectsInAreaResponse_Serialization_WorksCorrectly()
    {
        var original = new GetObjectsInAreaResponse
        {
            Objects = new[]
            {
                new MapObjectDto { Id = "obj1", X = 10, Y = 20, Width = 5, Height = 3 },
                new MapObjectDto { Id = "obj2", X = 15, Y = 25, Width = 4, Height = 2 }
            }
        };

        var serialized = MemoryPackSerializer.Serialize(original);
        var deserialized = MemoryPackSerializer.Deserialize<GetObjectsInAreaResponse>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized!.Objects.Length);
        Assert.Equal("obj1", deserialized.Objects[0].Id);
        Assert.Equal("obj2", deserialized.Objects[1].Id);
    }

    [Fact]
    public void GetRegionsInAreaRequest_Serialization_WorksCorrectly()
    {
        var original = new GetRegionsInAreaRequest
        {
            X = 5,
            Y = 15,
            Width = 50,
            Height = 75
        };

        var serialized = MemoryPackSerializer.Serialize(original);
        var deserialized = MemoryPackSerializer.Deserialize<GetRegionsInAreaRequest>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(original.X, deserialized!.X);
        Assert.Equal(original.Y, deserialized.Y);
        Assert.Equal(original.Width, deserialized.Width);
        Assert.Equal(original.Height, deserialized.Height);
    }

    [Fact]
    public void GetRegionsInAreaResponse_Serialization_WorksCorrectly()
    {
        var original = new GetRegionsInAreaResponse
        {
            Regions = new[]
            {
                new RegionDto { Id = 1, Name = "Region 1" },
                new RegionDto { Id = 2, Name = "Region 2" }
            }
        };

        var serialized = MemoryPackSerializer.Serialize(original);
        var deserialized = MemoryPackSerializer.Deserialize<GetRegionsInAreaResponse>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized!.Regions.Length);
        Assert.Equal(1, deserialized.Regions[0].Id);
        Assert.Equal("Region 1", deserialized.Regions[0].Name);
    }

    [Fact]
    public void ObjectEventDto_Serialization_WorksCorrectly()
    {
        var original = new ObjectEventDto
        {
            EventType = ObjectEventType.Added,
            Object = new MapObjectDto { Id = "obj1", X = 10, Y = 20, Width = 5, Height = 3 },
            Timestamp = DateTime.UtcNow
        };

        var serialized = MemoryPackSerializer.Serialize(original);
        var deserialized = MemoryPackSerializer.Deserialize<ObjectEventDto>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(original.EventType, deserialized!.EventType);
        Assert.Equal(original.Object.Id, deserialized.Object.Id);
        Assert.Equal(original.Timestamp, deserialized.Timestamp);
    }
}

