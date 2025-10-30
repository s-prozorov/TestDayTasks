namespace ObjectsLib;

public static class CoordinatesConverter
{
    private const double MapSize = 1000.0;
    private const double LongitudeRange = 360.0;
    private const double LatitudeRange = 180.0;

    // Радиус Земли в километрах
    private const double EarthRadiusKm = 6371.0;
    
    public static double MapXToLongitude(double x) => (x / MapSize) * LongitudeRange - 180;
    public static double MapYToLatitude(double y) => (y / MapSize) * LatitudeRange - 90;   
    
    public static double LongitudeToMapX(double longitude) => 
        (longitude + 180) / LongitudeRange * MapSize;
    
    public static double LatitudeToMapY(double latitude) => 
        (latitude + 90) / LatitudeRange * MapSize;
    
    /// <summary>
    /// Вычисляет расстояние в километрах между двумя точками на карте
    /// используя формулу Haversine
    /// </summary>
    public static double CalculateDistanceKm(double x1, double y1, double x2, double y2)
    {
        var lon1 = MapXToLongitude(x1);
        var lat1 = MapYToLatitude(y1);
        var lon2 = MapXToLongitude(x2);
        var lat2 = MapYToLatitude(y2);
        
        // Haversine
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        
        var lat1Rad = lat1 * Math.PI / 180.0;
        var lat2Rad = lat2 * Math.PI / 180.0;
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * 
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return EarthRadiusKm * c;
    }
    
    /// <summary>
    /// Вычисляет радиус окружности, описывающей прямоугольник в километрах.
    /// Используется для поиска объектов: GEORADIUS должен покрыть весь прямоугольник.
    /// </summary>
    public static double RectangleToCircleRadiusKm(
        double centerX, double centerY, 
        double halfWidth, double halfHeight)
    {
        // Радиус описанной окружности = расстояние от центра до угла прямоугольника
        return CalculateDistanceKm(
            centerX, centerY, 
            centerX + halfWidth, centerY + halfHeight);
    }
}