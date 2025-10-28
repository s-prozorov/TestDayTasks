using MapUnits.Core;
using Xunit;

namespace MapUnits.Tests;

public class MovementTests
{
    [Fact]
    public void GetDistance_ReturnsCorrectDistance()
    {
        var movement = new Movement("u1", 0, 0, 3, 4, 1.0f, DateTime.UtcNow);

        float distance = movement.GetDistance();

        Assert.Equal(5.0f, distance, 2);
    }

    [Fact]
    public void GetCurrentPosition_AtStart_ReturnsStartPosition()
    {
        var startTime = DateTime.UtcNow;
        var movement = new Movement("u1", 10, 20, 30, 40, 10.0f, startTime);

        var (x, y) = movement.GetCurrentPosition(startTime);

        Assert.Equal(10f, x, 2);
        Assert.Equal(20f, y, 2);
    }

    [Fact]
    public void GetCurrentPosition_HalfWay_ReturnsMiddlePosition()
    {
        var startTime = DateTime.UtcNow;
        var movement = new Movement("u1", 0, 0, 10, 0, 10.0f, startTime);

        var (x, y) = movement.GetCurrentPosition(startTime.AddSeconds(0.5));

        Assert.Equal(5f, x, 1);
        Assert.Equal(0f, y, 1);
    }

    [Fact]
    public void GetCurrentPosition_AfterCompletion_ReturnsTargetPosition()
    {
        var startTime = DateTime.UtcNow;
        var movement = new Movement("u1", 0, 0, 10, 10, 10.0f, startTime);

        var (x, y) = movement.GetCurrentPosition(startTime.AddSeconds(10));

        Assert.Equal(10f, x, 2);
        Assert.Equal(10f, y, 2);
    }

    [Fact]
    public void IsCompleted_BeforeEnd_ReturnsFalse()
    {
        var startTime = DateTime.UtcNow;
        var movement = new Movement("u1", 0, 0, 100, 0, 10.0f, startTime);

        bool completed = movement.IsCompleted(startTime.AddSeconds(5));

        Assert.False(completed);
    }

    [Fact]
    public void IsCompleted_AfterEnd_ReturnsTrue()
    {
        var startTime = DateTime.UtcNow;
        var movement = new Movement("u1", 0, 0, 10, 0, 10.0f, startTime);

        bool completed = movement.IsCompleted(startTime.AddSeconds(2));

        Assert.True(completed);
    }

    [Fact]
    public void IntersectsRect_StartInside_ReturnsTrue()
    {
        var movement = new Movement("u1", 5, 5, 50, 50, 1.0f, DateTime.UtcNow);

        bool intersects = movement.IntersectsRect(0, 0, 10, 10);

        Assert.True(intersects);
    }

    [Fact]
    public void IntersectsRect_EndInside_ReturnsTrue()
    {
        var movement = new Movement("u1", 50, 50, 5, 5, 1.0f, DateTime.UtcNow);

        bool intersects = movement.IntersectsRect(0, 0, 10, 10);

        Assert.True(intersects);
    }

    [Fact]
    public void IntersectsRect_LinePassesThrough_ReturnsTrue()
    {
        var movement = new Movement("u1", 0, 5, 20, 5, 1.0f, DateTime.UtcNow);

        bool intersects = movement.IntersectsRect(5, 0, 10, 10);

        Assert.True(intersects);
    }

    [Fact]
    public void IntersectsRect_NoIntersection_ReturnsFalse()
    {
        var movement = new Movement("u1", 0, 0, 5, 5, 1.0f, DateTime.UtcNow);

        bool intersects = movement.IntersectsRect(20, 20, 10, 10);

        Assert.False(intersects);
    }

    [Fact]
    public void IntersectsRect_DiagonalThrough_ReturnsTrue()
    {
        var movement = new Movement("u1", 0, 0, 20, 20, 1.0f, DateTime.UtcNow);

        bool intersects = movement.IntersectsRect(5, 5, 10, 10);

        Assert.True(intersects);
    }
}


