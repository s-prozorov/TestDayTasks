using Application.Features.Objects.Enums;
using Application.Features.Objects.Models;
using Application.Features.Objects.Services;
using Moq;
using StackExchange.Redis;

namespace Application.Tests.Features.Objects.Services;

public class ObjectsServiceTests
{
    private readonly ObjectsService _service;

    public ObjectsServiceTests()
    {
        var mockConnectionMultiplexer = ConnectionMultiplexer.Connect("localhost");
        _service = new ObjectsService(mockConnectionMultiplexer);
    }


    [Theory]
    [InlineData(100, 200, ObjectTypeEnum.Basa)]
    [InlineData(50, 75, ObjectTypeEnum.Mine)]
    [InlineData(0, 0, ObjectTypeEnum.TemporaryObject)]
    public async Task CreateObject_ShouldReturnId(short x, short y, ObjectTypeEnum type)
    {
        // Arrange
        var objectInfo = new ObjectInfo { X = x, Y = y, Type = type };

        // Act
        var id = await _service.CreateObject(objectInfo);

        // Assert
        Assert.True(id > 0);
        Assert.True(id < 1_000_000);
    }

    [Theory]
    [InlineData(0, 300, 400)]
    [InlineData(0, 0, 0)]
    [InlineData(0, 500, 600)]
    public async Task MoveObject_ShouldReturnTrue(long id, short x, short y)
    {
        var objectInfo = new ObjectInfo { X = x, Y = y, Type = ObjectTypeEnum.Mine };
        var correctId = await _service.CreateObject(objectInfo);
        // Act
        var result = await _service.MoveObject(correctId, x, y);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]
    public async Task DeleteObject_ShouldReturnBool(long id)
    {
        var objectInfo = new ObjectInfo { X = 200, Y = 300, Type = ObjectTypeEnum.Mine };
        var correctId = await _service.CreateObject(objectInfo);
        // Act
        var result = await _service.DeleteObject(correctId);

        // Assert
        Assert.IsType<bool>(result);
    }

    [Theory]
    [InlineData(100, 200, 50.0)]
    [InlineData(0, 0, 10.0)]
    [InlineData(500, 500, 100.0)]
    public async Task GetObjectsInZone_ShouldReturnArray(short x, short y, double radius)
    {
        // Arrange
        var center = new CenterModel { X = x, Y = y };

        // Act
        var result = await _service.GetObjectsInZone(center, radius);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ObjectInfo[]>(result);
    }
}

