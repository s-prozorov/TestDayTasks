namespace ObjectLayerEngine;

public static class CoordinateConverter
{
    private const double MapWidth = 1000.0;
    private const double MapHeight = 1000.0;
    private const double LonMin = -180.0;
    private const double LonMax = 180.0;
    private const double LatMin = -90.0;
    private const double LatMax = 90.0;

    public static (double lon, double lat) ConvertToGeo(int x, int y)
    {
        double normalizedX = x / MapWidth;
        double normalizedY = y / MapHeight;

        return (
            LonMin + (LonMax - LonMin) * normalizedX,
            LatMin + (LatMax - LatMin) * normalizedY
        );
    }
}