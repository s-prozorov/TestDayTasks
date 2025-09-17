using Application.Models;
using Microsoft.AspNetCore.Mvc;
using ObjectLayerEngine;
using RegionLayerEngine;
using StackExchange.Redis;
using TileMapEngine.Models;

namespace Application.Controller;

[ApiController]
[Route("api/[controller]")]
public class ObjectsController : ControllerBase
{
    private readonly TileLayer _tileLayer;
    private readonly ObjectLayer _objectLayer;
    private readonly RegionLayer _regionLayer;
    private readonly IConnectionMultiplexer _redis;
    private readonly string _streamName = "objectEvents";
    
    public ObjectsController(
        TileLayer tileLayer,
        ObjectLayer objectLayer,
        RegionLayer regionLayer,
        IConnectionMultiplexer redis)
    {
        _tileLayer = _tileLayer;
        _objectLayer = objectLayer;
        _regionLayer = regionLayer;
        _redis = redis;
    }

    [HttpPost]
    public async Task<IActionResult> CreateObject([FromBody] ObjectCreationRequest request)
    {
        var obj = new GameObject(
            Guid.NewGuid().ToString(),
            request.X,
            request.Y,
            request.Width,
            request.Height);

        if (!_tileLayer.CanPlaceObject(obj.X, obj.Y))
            return BadRequest("Cannot place object in this area");

        _objectLayer.AddObject(obj);
        await PublishEvent("create", obj);
        return CreatedAtAction(nameof(GetById), new { id = obj.Id }, obj);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> MoveObject(string id, [FromBody] MoveRequest request)
    {
        var obj = _objectLayer.GetById(id);
        if (obj == null) return NotFound();

        if (!_tileLayer.CanPlaceObject(request.NewX, request.NewY))
            return BadRequest("Cannot move object to this area");

        var newObj = new GameObject(id, request.NewX, request.NewY, obj.Width, obj.Height);
        _objectLayer.RemoveObject(id);
        _objectLayer.AddObject(newObj);

        await PublishEvent("move", newObj);
        return Ok(newObj);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteObject(string id)
    {
        var obj = _objectLayer.GetById(id);
        if (obj == null) return NotFound();

        _objectLayer.RemoveObject(id);
        await PublishEvent("delete", obj);
        return NoContent();
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var obj = _objectLayer.GetById(id);
        return obj != null ? Ok(obj) : NotFound();
    }

    [HttpGet]
    public IActionResult GetObjects([FromQuery] AreaSearchRequest request)
    {
        try
        {
            var objects = _objectLayer.GetObjectsInArea(
                request.X,
                request.Y,
                request.Radius);

            return Ok(objects);
        }
        catch (Exception ex)
        {
            return BadRequest("Error retrieving objects");
        }
    }

    private async Task PublishEvent(string eventType, GameObject obj)
    {
        var db = _redis.GetDatabase();
    
        var messageFields = new[]
        {
            new NameValueEntry("eventType", eventType),
            new NameValueEntry("objectId", obj.Id),
            new NameValueEntry("x", obj.X),
            new NameValueEntry("y", obj.Y),
            new NameValueEntry("width", obj.Width),
            new NameValueEntry("height", obj.Height),
            new NameValueEntry("timestamp", DateTime.UtcNow.Ticks)
        };

        await db.StreamAddAsync(
            _streamName,
            messageFields,
            RedisValue.Null,
            5000, // Максимум 5000 событий в потоке
            true, // Оптимизированное удаление старых записей
            CommandFlags.FireAndForget); // Ускорение записи
    }
}
