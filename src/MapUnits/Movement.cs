namespace MapUnits.Core;

public readonly struct Movement(string unitId, float startX, float startY, float targetX, float targetY, float speed, DateTime startTime)
{
    public string UnitId { get; } = unitId;
    public float StartX { get; } = startX;
    public float StartY { get; } = startY;
    public float TargetX { get; } = targetX;
    public float TargetY { get; } = targetY;
    public float Speed { get; } = speed;
    public DateTime StartTime { get; } = startTime;

    public float GetDistance()
    {
        var dx = TargetX - StartX;
        var dy = TargetY - StartY;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public (float x, float y) GetCurrentPosition(DateTime currentTime)
    {
        var elapsed = (float)(currentTime - StartTime).TotalSeconds;
        var distance = GetDistance();
        var travelTime = distance / Speed;

        if (elapsed >= travelTime) return (TargetX, TargetY);

        var t = elapsed / travelTime;
        return (StartX + (TargetX - StartX) * t, StartY + (TargetY - StartY) * t);
    }

    public bool IsCompleted(DateTime currentTime)
    {
        var elapsed = (float)(currentTime - StartTime).TotalSeconds;
        var totalTime = GetDistance() / Speed;
        return elapsed >= totalTime;
    }

    public bool IntersectsRect(float rectX, float rectY, float rectWidth, float rectHeight)
    {
        var x1 = StartX;
        var y1 = StartY;
        var x2 = TargetX;
        var y2 = TargetY;

        if (x1 >= rectX && x1 < rectX + rectWidth && y1 >= rectY && y1 < rectY + rectHeight)
            return true;
        if (x2 >= rectX && x2 < rectX + rectWidth && y2 >= rectY && y2 < rectY + rectHeight)
            return true;

        var left = rectX;
        var right = rectX + rectWidth;
        var top = rectY;
        var bottom = rectY + rectHeight;

        if (LineIntersectsSegment(x1, y1, x2, y2, left, top, right, top)) return true;
        if (LineIntersectsSegment(x1, y1, x2, y2, right, top, right, bottom)) return true;
        if (LineIntersectsSegment(x1, y1, x2, y2, right, bottom, left, bottom)) return true;
        if (LineIntersectsSegment(x1, y1, x2, y2, left, bottom, left, top)) return true;

        return false;
    }

    private static bool LineIntersectsSegment(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
    {
        var denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (MathF.Abs(denom) < 1e-10f)
            return false;

        var t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        var u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denom;

        return (t >= 0 && t <= 1 && u >= 0 && u <= 1);
    }
}
