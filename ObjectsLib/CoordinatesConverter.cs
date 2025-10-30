namespace ObjectsLib;

public static class CoordinatesConverter
{
    private const double MapSize = 1000.0;
    private const double LongitudeRange = 360.0;
    private const double LatitudeRange = 180.0;

    public static double MapXToLongitude(double x) => (x / MapSize) * LongitudeRange - 180;
    public static double MapYToLatitude(double y) => (y / MapSize) * LatitudeRange - 90;   
    
    public static double LongitudeToMapX(double longitude) => 
        (longitude + 180) / LongitudeRange * MapSize;
    
    public static double LatitudeToMapY(double latitude) => 
        (latitude + 90) / LatitudeRange * MapSize;
}