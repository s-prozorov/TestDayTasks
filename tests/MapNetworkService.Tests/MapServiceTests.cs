using Xunit;
using Moq;
using MapNetworkService.Services;
using MapNetworkService.Contracts;
using MapObjects;
using MapRegions;
using MapUnits.Core;

namespace MapNetworkService.Tests;

public class MapServiceTests
{
    [Fact]
    public async Task GetObjectsInAreaAsync_ValidRequest_ReturnsObjects()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockRegionLayer = new Mock<IRegionLayer>();
        var mockUnitService = new Mock<IUnitMovementService>();

        var objects = new List<MapObject>
        {
            new MapObject("obj1", 10, 20, 5, 3),
            new MapObject("obj2", 15, 25, 4, 2)
        };

        mockObjectLayer
            .Setup(x => x.GetObjectsInAreaAsync(10, 20, 100, 100))
            .ReturnsAsync(objects);

        var service = new MapService(mockObjectLayer.Object, mockRegionLayer.Object, mockUnitService.Object);

        var request = new GetObjectsInAreaRequest { X = 10, Y = 20, Width = 100, Height = 100 };
        var response = await service.GetObjectsInAreaAsync(request);

        Assert.Equal(2, response.Objects.Length);
        Assert.Equal("obj1", response.Objects[0].Id);
        Assert.Equal("obj2", response.Objects[1].Id);
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_EmptyArea_ReturnsEmptyList()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockRegionLayer = new Mock<IRegionLayer>();
        var mockUnitService = new Mock<IUnitMovementService>();

        mockObjectLayer
            .Setup(x => x.GetObjectsInAreaAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<MapObject>());

        var service = new MapService(mockObjectLayer.Object, mockRegionLayer.Object, mockUnitService.Object);

        var request = new GetObjectsInAreaRequest { X = 0, Y = 0, Width = 10, Height = 10 };
        var response = await service.GetObjectsInAreaAsync(request);

        Assert.Empty(response.Objects);
    }

    [Fact]
    public async Task GetRegionsInAreaAsync_ValidRequest_ReturnsRegions()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockRegionLayer = new Mock<IRegionLayer>();
        var mockUnitService = new Mock<IUnitMovementService>();

        var regionIds = new HashSet<ushort> { 1, 2, 3 };
        mockRegionLayer
            .Setup(x => x.GetRegionsInArea(10, 20, 100, 100))
            .Returns(regionIds);

        mockRegionLayer
            .Setup(x => x.GetRegionMetadata(It.IsAny<ushort>()))
            .Returns((ushort id) => new RegionMetadata(id, $"Region {id}"));

        var service = new MapService(mockObjectLayer.Object, mockRegionLayer.Object, mockUnitService.Object);

        var request = new GetRegionsInAreaRequest { X = 10, Y = 20, Width = 100, Height = 100 };
        var response = await service.GetRegionsInAreaAsync(request);

        Assert.Equal(3, response.Regions.Length);
        Assert.Contains(response.Regions, r => r.Id == 1);
        Assert.Contains(response.Regions, r => r.Id == 2);
        Assert.Contains(response.Regions, r => r.Id == 3);
    }

    [Fact]
    public async Task GetRegionsInAreaAsync_EmptyArea_ReturnsEmptyList()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockRegionLayer = new Mock<IRegionLayer>();
        var mockUnitService = new Mock<IUnitMovementService>();

        mockRegionLayer
            .Setup(x => x.GetRegionsInArea(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new HashSet<ushort>());

        var service = new MapService(mockObjectLayer.Object, mockRegionLayer.Object, mockUnitService.Object);

        var request = new GetRegionsInAreaRequest { X = 0, Y = 0, Width = 10, Height = 10 };
        var response = await service.GetRegionsInAreaAsync(request);

        Assert.Empty(response.Regions);
    }

    [Fact]
    public void Constructor_NullObjectLayer_ThrowsArgumentNullException()
    {
        var mockRegionLayer = new Mock<IRegionLayer>();
        var mockUnitService = new Mock<IUnitMovementService>();

        Assert.Throws<ArgumentNullException>(() => new MapService(null!, mockRegionLayer.Object, mockUnitService.Object));
    }

    [Fact]
    public void Constructor_NullRegionLayer_ThrowsArgumentNullException()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockUnitService = new Mock<IUnitMovementService>();

        Assert.Throws<ArgumentNullException>(() => new MapService(mockObjectLayer.Object, null!, mockUnitService.Object));
    }
}

