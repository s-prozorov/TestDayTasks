namespace MapObjects;

public static class CoordinateConverter
{
    private const double BaseLatitude = 0.0;
    private const double BaseLongitude = 0.0;
    public const double DegreesPerTile = 0.00001;

    public static (double latitude, double longitude) ToGeoCoordinates(int x, int y)
    {
        double latitude = BaseLatitude + y * DegreesPerTile;
        double longitude = BaseLongitude + x * DegreesPerTile;
        return (latitude, longitude);
    }

    public static (int x, int y) FromGeoCoordinates(double latitude, double longitude)
    {
        int x = (int)Math.Round((longitude - BaseLongitude) / DegreesPerTile);
        int y = (int)Math.Round((latitude - BaseLatitude) / DegreesPerTile);
        return (x, y);
    }
}

