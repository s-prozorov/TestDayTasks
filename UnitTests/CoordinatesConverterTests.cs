using ObjectsLib;

namespace UnitTests;

public class CoordinatesConverterTests
{
    [Theory]
    [InlineData(0, -180)]
    [InlineData(500, 0)]
    [InlineData(1000, 180)]
    [InlineData(250, -90)]
    [InlineData(750, 90)]
    public void MapXToLongitude_ConvertsCorrectly(double x, double expectedLongitude)
    {
        // Act
        var longitude = CoordinatesConverter.MapXToLongitude(x);

        // Assert
        Assert.Equal(expectedLongitude, longitude, precision: 10);
    }

    [Theory]
    [InlineData(0, -90)]
    [InlineData(500, 0)]
    [InlineData(1000, 90)]
    [InlineData(250, -45)]
    [InlineData(750, 45)]
    public void MapYToLatitude_ConvertsCorrectly(double y, double expectedLatitude)
    {
        // Act
        var latitude = CoordinatesConverter.MapYToLatitude(y);

        // Assert
        Assert.Equal(expectedLatitude, latitude, precision: 10);
    }

    [Theory]
    [InlineData(-180, 0)]
    [InlineData(0, 500)]
    [InlineData(180, 1000)]
    [InlineData(-90, 250)]
    [InlineData(90, 750)]
    public void LongitudeToMapX_ConvertsCorrectly(double longitude, double expectedX)
    {
        // Act
        var x = CoordinatesConverter.LongitudeToMapX(longitude);

        // Assert
        Assert.Equal(expectedX, x, precision: 10);
    }

    [Theory]
    [InlineData(-90, 0)]
    [InlineData(0, 500)]
    [InlineData(90, 1000)]
    [InlineData(-45, 250)]
    [InlineData(45, 750)]
    public void LatitudeToMapY_ConvertsCorrectly(double latitude, double expectedY)
    {
        // Act
        var y = CoordinatesConverter.LatitudeToMapY(latitude);

        // Assert
        Assert.Equal(expectedY, y, precision: 10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(999)]
    [InlineData(1000)]
    public void MapXToLongitude_AndBack_ReturnsOriginalValue(double originalX)
    {
        // Act
        var longitude = CoordinatesConverter.MapXToLongitude(originalX);
        var x = CoordinatesConverter.LongitudeToMapX(longitude);

        // Assert
        Assert.Equal(originalX, x, precision: 10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(999)]
    [InlineData(1000)]
    public void MapYToLatitude_AndBack_ReturnsOriginalValue(double originalY)
    {
        // Act
        var latitude = CoordinatesConverter.MapYToLatitude(originalY);
        var y = CoordinatesConverter.LatitudeToMapY(latitude);

        // Assert
        Assert.Equal(originalY, y, precision: 10);
    }

    [Theory]
    [InlineData(-180)]
    [InlineData(-90)]
    [InlineData(0)]
    [InlineData(90)]
    [InlineData(180)]
    public void LongitudeToMapX_AndBack_ReturnsOriginalValue(double originalLongitude)
    {
        // Act
        var x = CoordinatesConverter.LongitudeToMapX(originalLongitude);
        var longitude = CoordinatesConverter.MapXToLongitude(x);

        // Assert
        Assert.Equal(originalLongitude, longitude, precision: 10);
    }

    [Theory]
    [InlineData(-90)]
    [InlineData(-45)]
    [InlineData(0)]
    [InlineData(45)]
    [InlineData(90)]
    public void LatitudeToMapY_AndBack_ReturnsOriginalValue(double originalLatitude)
    {
        // Act
        var y = CoordinatesConverter.LatitudeToMapY(originalLatitude);
        var latitude = CoordinatesConverter.MapYToLatitude(y);

        // Assert
        Assert.Equal(originalLatitude, latitude, precision: 10);
    }
    
    [Fact]
    public void CalculateDistanceKm_OneTileHorizontallyAtEquator_ReturnsApproximately40Km()
    {
        // Arrange
        double x1 = 500, y1 = 500; // Экватор
        double x2 = 501, y2 = 500; // +1 тайл по X
        
        // 1 тайл по X = 0.36° × 111 км/° ≈ 40 км
        
        // Act
        var distance = CoordinatesConverter.CalculateDistanceKm(x1, y1, x2, y2);
        
        // Assert
        Assert.InRange(distance, 38, 42);
    }
    
    [Fact]
    public void RectangleToCircleRadiusKm_SquareTenByTenAtEquator_ReturnsCorrectRadius()
    {
        // Arrange
        double centerX = 500, centerY = 500; // Экватор
        double halfWidth = 5;
        double halfHeight = 5;
        
        // Диагональ от центра до угла = sqrt(5² + 5²) ≈ 7.07 тайлов
        // На экваторе:
        // - По X: ~40 км/тайл
        // - По Y: ~20 км/тайл
        // Расстояние до угла ≈ sqrt((5×40)² + (5×20)²) ≈ sqrt(40000 + 10000) ≈ 224 км
        
        // Act
        var radiusKm = CoordinatesConverter.RectangleToCircleRadiusKm(
            centerX, centerY, halfWidth, halfHeight);
        
        // Assert
        Assert.InRange(radiusKm, 210, 240);
    }
}