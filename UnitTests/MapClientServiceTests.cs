using MapLib;
using MapLib.Interfaces;
using MapLib.Models;
using MapService;
using MapService.Interfaces;
using MapService.Requests;
using Moq;
using ObjectsLib.Interfaces;
using ObjectsLib.Models;

namespace UnitTests;

public class MapClientServiceTests
{
    private readonly Mock<IMap> _mapMock;
    private readonly Mock<IObjectStorage> _objectStorageMock;
    private readonly Mock<IMapEventBroadcaster> _broadcasterMock;
    private readonly MapClientService _service;

    public MapClientServiceTests()
    {
        _mapMock = new Mock<IMap>();
        _objectStorageMock = new Mock<IObjectStorage>();
        _broadcasterMock = new Mock<IMapEventBroadcaster>();
        _service = new MapClientService(_mapMock.Object, _objectStorageMock.Object, _broadcasterMock.Object);
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_ReturnsObjectsInRectangle()
    {
        // Arrange
        var request = new GetObjectsInAreaRequest
        {
            X1 = 10,
            Y1 = 10,
            X2 = 20,
            Y2 = 20
        };

        var objects = new List<ObjectData>
        {
            new() { Id = "1", X = 15, Y = 15, Type = ObjectType.PlayerBase, OwnerId = 1 },
            new() { Id = "2", X = 25, Y = 25, Type = ObjectType.Mine, OwnerId = 2 }, // Вне области
            new() { Id = "3", X = 12, Y = 18, Type = ObjectType.PlayerBase, OwnerId = 1 }
        };

        _objectStorageMock
            .Setup(x => x.GetObjectsInZoneAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(objects);

        // Act
        var result = await _service.GetObjectsInAreaAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Objects.Length);
        Assert.Contains(result.Objects, o => o.Id == "1");
        Assert.Contains(result.Objects, o => o.Id == "3");
        Assert.DoesNotContain(result.Objects, o => o.Id == "2");
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_EmptyArea_ReturnsEmptyArray()
    {
        // Arrange
        var request = new GetObjectsInAreaRequest
        {
            X1 = 10,
            Y1 = 10,
            X2 = 20,
            Y2 = 20
        };

        _objectStorageMock
            .Setup(x => x.GetObjectsInZoneAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new List<ObjectData>());

        // Act
        var result = await _service.GetObjectsInAreaAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Objects);
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_ObjectsOnBoundary_Included()
    {
        // Arrange
        var request = new GetObjectsInAreaRequest
        {
            X1 = 0,
            Y1 = 0,
            X2 = 10,
            Y2 = 10
        };

        var objects = new List<ObjectData>
        {
            new() { Id = "1", X = 0, Y = 0, Type = ObjectType.PlayerBase, OwnerId = 1 },
            new() { Id = "2", X = 10, Y = 10, Type = ObjectType.PlayerBase, OwnerId = 1 },
            new() { Id = "3", X = 5, Y = 5, Type = ObjectType.PlayerBase, OwnerId = 1 }
        };

        _objectStorageMock
            .Setup(x => x.GetObjectsInZoneAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(objects);

        // Act
        var result = await _service.GetObjectsInAreaAsync(request);

        // Assert
        Assert.Equal(3, result.Objects.Length);
    }

    [Fact]
    public async Task GetRegionsInAreaAsync_SmallArea_ReturnsRegions()
    {
        // Arrange
        var request = new GetRegionsInAreaRequest
        {
            X1 = 0,
            Y1 = 0,
            X2 = 10,
            Y2 = 10
        };

        _mapMock.Setup(x => x.GetTerritoryId(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int x, int y) => x % 3); // Разные территории

        _mapMock.Setup(x => x.GetTerritoryInfo(It.IsAny<int>()))
            .Returns((int id) => new TerritoryInfo(id, id * 10, $"Territory{id}"));

        // Act
        var result = await _service.GetRegionsInAreaAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Regions);
    }

    [Fact]
    public async Task GetRegionsInAreaAsync_LargeArea_UsesOptimization()
    {
        // Arrange - область больше 2500 тайлов
        var request = new GetRegionsInAreaRequest
        {
            X1 = 0,
            Y1 = 0,
            X2 = 100,
            Y2 = 100
        };

        _mapMock.Setup(x => x.GetTerritoryId(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(1);

        _mapMock.Setup(x => x.GetTerritoryInfo(1))
            .Returns(new TerritoryInfo(1, 10, "Territory1"));

        // Act
        var result = await _service.GetRegionsInAreaAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Regions);
        Assert.Equal(1, result.Regions[0].Id);
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_InvalidCoordinates_ThrowsException()
    {
        // Arrange
        var request = new GetObjectsInAreaRequest
        {
            X1 = -1,
            Y1 = 0,
            X2 = 10,
            Y2 = 10
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _service.GetObjectsInAreaAsync(request));
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_OutOfBounds_ThrowsException()
    {
        // Arrange
        var request = new GetObjectsInAreaRequest
        {
            X1 = 0,
            Y1 = 0,
            X2 = 1000, // >= Map.Width (1000)
            Y2 = 10
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _service.GetObjectsInAreaAsync(request));
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_InvalidRectangle_ThrowsException()
    {
        // Arrange - X1 > X2
        var request = new GetObjectsInAreaRequest
        {
            X1 = 20,
            Y1 = 0,
            X2 = 10,
            Y2 = 10
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.GetObjectsInAreaAsync(request));
    }

    [Fact]
    public async Task CreateObjectAsync_ValidRequest_CreatesAndBroadcasts()
    {
        // Arrange
        var request = new CreateObjectRequest
        {
            X = 10,
            Y = 20,
            ObjectTypeId = (int)ObjectType.PlayerBase
        };

        var createdObject = new ObjectData
        {
            Id = "test-id",
            X = 10,
            Y = 20,
            Type = ObjectType.PlayerBase,
            OwnerId = null
        };

        _objectStorageMock
            .Setup(x => x.CreateObjectAsync(It.IsAny<ObjectData>()))
            .ReturnsAsync(createdObject);

        // Act
        var result = await _service.CreateObjectAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ObjectId);
        _objectStorageMock.Verify(x => x.CreateObjectAsync(It.IsAny<ObjectData>()), Times.Once);
        _broadcasterMock.Verify(x => x.BroadcastObjectAddedAsync(It.IsAny<MapService.Events.ObjectAddedEvent>()), Times.Once);
    }

    [Fact]
    public async Task DeleteObjectAsync_ValidRequest_DeletesAndBroadcasts()
    {
        // Arrange
        var request = new DeleteObjectRequest
        {
            ObjectId = "test-id"
        };

        _objectStorageMock
            .Setup(x => x.DeleteObjectAsync("test-id"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteObjectAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _objectStorageMock.Verify(x => x.DeleteObjectAsync("test-id"), Times.Once);
        _broadcasterMock.Verify(x => x.BroadcastObjectDeletedAsync(It.IsAny<MapService.Events.ObjectDeletedEvent>()), Times.Once);
    }

    [Fact]
    public async Task UpdateObjectAsync_ValidRequest_UpdatesAndBroadcasts()
    {
        // Arrange
        var request = new UpdateObjectRequest
        {
            ObjectId = "test-id",
            X = 15,
            Y = 25
        };

        var existingObject = new ObjectData
        {
            Id = "test-id",
            X = 10,
            Y = 20,
            Type = ObjectType.PlayerBase,
            OwnerId = 1
        };

        _objectStorageMock
            .Setup(x => x.GetObjectAsync("test-id"))
            .ReturnsAsync(existingObject);

        _objectStorageMock
            .Setup(x => x.UpdateObjectAsync(It.IsAny<ObjectData>()))
            .ReturnsAsync((ObjectData obj) => obj);

        // Act
        var result = await _service.UpdateObjectAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _objectStorageMock.Verify(x => x.UpdateObjectAsync(It.Is<ObjectData>(o => o.X == 15 && o.Y == 25)), Times.Once);
        _broadcasterMock.Verify(x => x.BroadcastObjectUpdatedAsync(It.IsAny<MapService.Events.ObjectUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task UpdateObjectAsync_ObjectNotFound_ThrowsException()
    {
        // Arrange
        var request = new UpdateObjectRequest
        {
            ObjectId = "non-existent",
            X = 15,
            Y = 25
        };

        _objectStorageMock
            .Setup(x => x.GetObjectAsync("non-existent"))
            .ReturnsAsync((ObjectData?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.UpdateObjectAsync(request));
    }
}