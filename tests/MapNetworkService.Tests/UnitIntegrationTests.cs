using Xunit;
using Moq;
using MagicOnion;
using MapNetworkService.Services;
using MapNetworkService.Contracts;
using MapObjects;
using MapRegions;
using MapUnits.Core;
using MapSurface;

namespace MapNetworkService.Tests;

public class UnitIntegrationTests
{
    [Fact]
    public async Task GetUnitsInAreaAsync_WithUnits_ReturnsUnitsAndMovements()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockRegionLayer = new Mock<IRegionLayer>();
        var unitService = new UnitMovementService();

        var unit = new Unit("u1", 10, 20, 5.0f);
        unitService.AddUnit(unit);
        unitService.StartMovement("u1", 30, 40);

        var service = new MapService(mockObjectLayer.Object, mockRegionLayer.Object, unitService);

        var request = new GetObjectsInAreaRequest { X = 0, Y = 0, Width = 50, Height = 50 };
        var response = await service.GetUnitsInAreaAsync(request);

        Assert.Single(response.Units);
        Assert.Single(response.Movements);
        Assert.Equal("u1", response.Units[0].Id);
    }

    [Fact]
    public void StartUnitMovementAsync_ValidRequest_StartsMovement()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockRegionLayer = new Mock<IRegionLayer>();
        var unitService = new UnitMovementService();

        unitService.AddUnit(new Unit("u1", 10, 20));

        var service = new MapService(mockObjectLayer.Object, mockRegionLayer.Object, unitService);

        var request = new StartMovementRequest { UnitId = "u1", TargetX = 30, TargetY = 40 };
        service.StartUnitMovementAsync(request);

        var movements = unitService.GetMovementsInArea(0, 0, 100, 100);
        Assert.Single(movements);
    }

    [Fact]
    public void StartUnitMovementAsync_WithSurfaceLayer_ValidatesWalkability()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockRegionLayer = new Mock<IRegionLayer>();
        var surface = new SurfaceLayer(100, 100);
        
        surface.SetTileType(50, 50, TileType.Mountain);
        
        var unitService = new UnitMovementService(100, 32, surface);
        unitService.AddUnit(new Unit("u1", 10, 10));

        var service = new MapService(mockObjectLayer.Object, mockRegionLayer.Object, unitService);

        var request = new StartMovementRequest { UnitId = "u1", TargetX = 50, TargetY = 50 };
        
        Assert.Throws<ReturnStatusException>(() => service.StartUnitMovementAsync(request));
    }

    [Fact]
    public void StartUnitMovementAsync_OutOfBounds_ThrowsException()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockRegionLayer = new Mock<IRegionLayer>();
        var surface = new SurfaceLayer(100, 100);
        var unitService = new UnitMovementService(100, 32, surface);
        
        unitService.AddUnit(new Unit("u1", 10, 10));

        var service = new MapService(mockObjectLayer.Object, mockRegionLayer.Object, unitService);

        var request = new StartMovementRequest { UnitId = "u1", TargetX = 200, TargetY = 200 };
        
        Assert.Throws<ReturnStatusException>(() => service.StartUnitMovementAsync(request));
    }

    [Fact]
    public async Task GetUnitsInAreaAsync_EmptyArea_ReturnsEmpty()
    {
        var mockObjectLayer = new Mock<IMapObjectLayer>();
        var mockRegionLayer = new Mock<IRegionLayer>();
        var unitService = new UnitMovementService();

        var service = new MapService(mockObjectLayer.Object, mockRegionLayer.Object, unitService);

        var request = new GetObjectsInAreaRequest { X = 0, Y = 0, Width = 10, Height = 10 };
        var response = await service.GetUnitsInAreaAsync(request);

        Assert.Empty(response.Units);
        Assert.Empty(response.Movements);
    }
}

