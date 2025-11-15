namespace MapSystem.Core.ObjectLayer;

internal static class CoordinateConverter
{
    private const double MinLatitude = -85.05112878;
    private const double MaxLatitude = 85.05112878;
    private const double MinLongitude = -180.0;
    private const double MaxLongitude = 180.0;

    private const double LatitudeRange = MaxLatitude - MinLatitude;
    private const double LongitudeRange = MaxLongitude - MinLongitude;

    private const int DefaultMapWidth = 1000;
    private const int DefaultMapHeight = 1000;

    public static (double latitude, double longitude) ToGeoCoordinates(
        int x,
        int y,
        int mapWidth = DefaultMapWidth,
        int mapHeight = DefaultMapHeight)
    {
        var normalizedX = x / (double)mapWidth;
        var longitude = normalizedX * LongitudeRange + MinLongitude;
        var normalizedY = y / (double)mapHeight;
        var latitude = normalizedY * LatitudeRange + MinLatitude;
        return (latitude, longitude);
    }

    public static (int x, int y) FromGeoCoordinates(
        double latitude,
        double longitude,
        int mapWidth = DefaultMapWidth,
        int mapHeight = DefaultMapHeight)
    {
        var normalizedX = (longitude - MinLongitude) / LongitudeRange;
        var x = (int)Math.Round(normalizedX * mapWidth);
        var normalizedY = (latitude - MinLatitude) / LatitudeRange;
        var y = (int)Math.Round(normalizedY * mapHeight);
        x = Math.Clamp(x, 0, mapWidth - 1);
        y = Math.Clamp(y, 0, mapHeight - 1);

        return (x, y);
    }

    public static double CalculateSearchRadiusKm(int width, int height)
    {
        var diagonalTiles = Math.Sqrt(width * width + height * height);
        var diagonalDegrees = diagonalTiles / DefaultMapWidth * LongitudeRange;
        var radiusKm = diagonalDegrees * 111.0 * 1.5;
        return radiusKm;
    }
}