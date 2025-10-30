using MapLib;
using MapService.Requests;
using ObjectsLib.Models;
using RectangleAreaRequest = MapService.Requests.Base.RectangleAreaRequest;

namespace MapService.Extensions;

public static class RequestValidationExtensions
{
    public static void ValidateCoordinates(this RectangleAreaRequest request)
    {
        var missingField = request switch
        {
            { X1: null } => nameof(request.X1),
            { Y1: null } => nameof(request.Y1),
            { X2: null } => nameof(request.X2),
            { Y2: null } => nameof(request.Y2),
            _ => null
        };

        if (missingField != null)
            throw new ArgumentException($"Required field '{missingField}' is empty.", missingField);
        
        // Проверка отрицательных координат
        if (request.X1 < 0 || request.Y1 < 0 || request.X2 < 0 || request.Y2 < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"Coordinates cannot be negative. Received: X1={request.X1}, Y1={request.Y1}, X2={request.X2}, Y2={request.Y2}");
        }
        
        // Проверка выхода за границы карты
        if (request.X1 >= Map.Width || request.X2 >= Map.Width ||
            request.Y1 >= Map.Height || request.Y2 >= Map.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"Coordinates are out of map bounds (0-{Map.Width - 1}, 0-{Map.Height - 1}). " +
                $"Received: X1={request.X1}, Y1={request.Y1}, X2={request.X2}, Y2={request.Y2}");
        }
        
        // Проверка корректности прямоугольника
        if (request.X1 > request.X2)
        {
            throw new ArgumentException(
                $"Invalid rectangle: X1 ({request.X1}) must be less than or equal to X2 ({request.X2})",
                nameof(request));
        }
        
        if (request.Y1 > request.Y2)
        {
            throw new ArgumentException(
                $"Invalid rectangle: Y1 ({request.Y1}) must be less than or equal to Y2 ({request.Y2})",
                nameof(request));
        }
    }

    public static void Validate(this DeleteObjectRequest request)
    {
        var missingField = request switch
        {
            { ObjectId: null } => nameof(request.ObjectId),
            _ => null
        };
        
        if (missingField != null)
            throw new ArgumentException($"Required field '{missingField}' is empty.", missingField);
    }
    
    public static void Validate(this UpdateObjectRequest request)
    {
        var missingField = request switch
        {
            { ObjectId: null } => nameof(request.ObjectId),
            { X: null } => nameof(request.X),
            { Y: null } => nameof(request.Y),
            _ => null
        };
        
        if (request.X < 0 || request.X >= Map.Width || 
            request.Y < 0 || request.Y >= Map.Height)
            throw new ArgumentOutOfRangeException(nameof(request), 
                "Coordinates are out of map bounds");
        
        if (missingField != null)
            throw new ArgumentException($"Required field '{missingField}' is empty.", missingField);
    }

    public static void Validate(this CreateObjectRequest request)
    {
        var missingField = request switch
        {
            { X: null } => nameof(request.X),
            { Y: null } => nameof(request.Y),
            { ObjectTypeId: null } => nameof(request.ObjectTypeId),
            _ => null
        };

        if (missingField != null)
            throw new ArgumentException($"Required field '{missingField}' is empty.", missingField);

        if (request.X < 0 || request.X >= Map.Width || 
            request.Y < 0 || request.Y >= Map.Height)
            throw new ArgumentOutOfRangeException(nameof(request), 
                "Coordinates are out of map bounds");

        if (!Enum.IsDefined(typeof(ObjectType), request.ObjectTypeId.Value))
            throw new ArgumentException(
                $"Invalid object type: {request.ObjectTypeId}",
                nameof(request.ObjectTypeId));
    }
}