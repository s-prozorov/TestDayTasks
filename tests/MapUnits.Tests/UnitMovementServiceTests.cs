using MapUnits.Core;
using Xunit;

namespace MapUnits.Tests;

public class UnitMovementServiceTests : IDisposable
{
    private readonly UnitMovementService _service;

    public UnitMovementServiceTests()
    {
        _service = new UnitMovementService(updateIntervalMs: 50);
    }

    public void Dispose()
    {
        _service?.Dispose();
    }

    [Fact]
    public void AddUnit_AddsUnitSuccessfully()
    {
        var unit = new Unit("u1", 10, 20);

        _service.AddUnit(unit);

        var retrieved = _service.GetUnit("u1");
        Assert.NotNull(retrieved);
        Assert.Equal("u1", retrieved.Id);
    }

    [Fact]
    public void GetUnit_NonExistent_ReturnsNull()
    {
        var result = _service.GetUnit("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void StartMovement_ValidUnit_StartsMovement()
    {
        var unit = new Unit("u1", 0, 0, 10.0f);
        _service.AddUnit(unit);

        _service.StartMovement("u1", 100, 0);

        var movements = _service.GetMovementsInArea(0, 0, 100, 100);
        Assert.Single(movements);
        Assert.Equal("u1", movements[0].UnitId);
    }

    [Fact]
    public void StartMovement_NonExistentUnit_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => 
            _service.StartMovement("nonexistent", 10, 10));
    }

    [Fact]
    public async Task StartMovement_UnitMovesOverTime()
    {
        var unit = new Unit("u1", 0, 0, 100.0f);
        _service.AddUnit(unit);

        _service.StartMovement("u1", 10, 0);

        await Task.Delay(200);

        var retrievedUnit = _service.GetUnit("u1");
        Assert.NotNull(retrievedUnit);
        Assert.True(retrievedUnit.X > 0);
    }

    [Fact]
    public async Task StartMovement_CompletesMovement()
    {
        var unit = new Unit("u1", 0, 0, 1000.0f);
        _service.AddUnit(unit);

        _service.StartMovement("u1", 10, 0);

        await Task.Delay(500);

        var retrievedUnit = _service.GetUnit("u1");
        Assert.NotNull(retrievedUnit);
        Assert.Equal(10f, retrievedUnit.X, 1);
        Assert.Equal(0f, retrievedUnit.Y, 1);
    }

    [Fact]
    public void GetUnitsInArea_ReturnsUnitsInArea()
    {
        _service.AddUnit(new Unit("u1", 5, 5));
        _service.AddUnit(new Unit("u2", 15, 15));
        _service.AddUnit(new Unit("u3", 25, 25));

        var units = _service.GetUnitsInArea(0, 0, 20, 20);

        Assert.Equal(2, units.Count);
        Assert.Contains(units, u => u.Id == "u1");
        Assert.Contains(units, u => u.Id == "u2");
    }

    [Fact]
    public void GetUnitsInArea_EmptyArea_ReturnsEmpty()
    {
        _service.AddUnit(new Unit("u1", 100, 100));

        var units = _service.GetUnitsInArea(0, 0, 10, 10);

        Assert.Empty(units);
    }

    [Fact]
    public void GetMovementsInArea_ReturnsMovementsThroughArea()
    {
        var unit1 = new Unit("u1", 0, 5, 1.0f);
        var unit2 = new Unit("u2", 100, 100, 1.0f);
        
        _service.AddUnit(unit1);
        _service.AddUnit(unit2);

        _service.StartMovement("u1", 20, 5);
        _service.StartMovement("u2", 200, 200);

        var movements = _service.GetMovementsInArea(5, 0, 10, 10);

        Assert.Single(movements);
        Assert.Equal("u1", movements[0].UnitId);
    }

    [Fact]
    public void StartMovement_SpeedVariation_BetweenRange()
    {
        var unit = new Unit("u1", 0, 0, 10.0f);
        _service.AddUnit(unit);

        _service.StartMovement("u1", 100, 0);

        var movements = _service.GetMovementsInArea(0, 0, 100, 100);
        float speed = movements[0].Speed;

        Assert.InRange(speed, 7.5f, 12.5f);
    }

    [Fact]
    public void GetUnitsInArea_LargeScale_PerformsEfficiently()
    {
        for (int i = 0; i < 10000; ++i)
        {
            _service.AddUnit(new Unit($"u{i}", i % 1000, i / 1000));
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var units = _service.GetUnitsInArea(0, 0, 200, 50);
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 100);
        Assert.NotEmpty(units);
    }
}

