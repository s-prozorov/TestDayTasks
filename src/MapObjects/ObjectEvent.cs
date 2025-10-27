namespace MapObjects;

public sealed class ObjectEvent
{
    public ObjectEventType EventType { get; }
    public MapObject Object { get; }
    public DateTime Timestamp { get; }

    public ObjectEvent(ObjectEventType eventType, MapObject obj)
    {
        EventType = eventType;
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
        Timestamp = DateTime.UtcNow;
    }
}

