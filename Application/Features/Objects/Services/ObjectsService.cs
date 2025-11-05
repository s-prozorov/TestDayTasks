using Application.Features.Objects.Models;
using StackExchange.Redis;
using System.Text.Json;


namespace Application.Features.Objects.Services;

public class ObjectsService
{
    private readonly IDatabase _RDB;

    public ObjectsService(IConnectionMultiplexer distributedCache)
    {
        _RDB = distributedCache.GetDatabase();
    }

    public double ConvertFromShortToDouble(short value)
    {
        return value / 10_000.0;
    }

    public short ConvertFromDoubleToShort(double value)
    {
        return (short)(value * 10_000);
    }

    public async Task<long> CreateObject(ObjectInfo objectInfo)
    {
        objectInfo.Id = Random.Shared.NextInt64(1, 1_000_000);

        var transaction = _RDB.CreateTransaction();

        await _RDB.StringSetAsync("objects_id_" + objectInfo.Id.ToString(), JsonSerializer.Serialize(objectInfo));
        await _RDB.GeoAddAsync("objects", ConvertFromShortToDouble(objectInfo.X), ConvertFromShortToDouble(objectInfo.Y), objectInfo.Id);

        if (transaction.Execute())
        {

            return objectInfo.Id.Value;
        }
        else
        {
            throw new Exception("Создание объекта не сработало.");
        }
    }

    public async Task<bool> MoveObject(long id, short x, short y)
    {
        var objectInfo = await _RDB.StringGetAsync("objects_id_" + id.ToString());
        if (objectInfo.HasValue == false) throw new NullReferenceException($"Объекта с Id = {id} не существует");

        var objectInfoObject = JsonSerializer.Deserialize<ObjectInfo>(objectInfo.ToString());
        if (objectInfoObject is null) throw new NullReferenceException();

        objectInfoObject.X = x;
        objectInfoObject.Y = y;

        await _RDB.StringSetAsync("objects_id_" + id.ToString(), JsonSerializer.Serialize(objectInfoObject));

        return true;
    }

    public async Task<bool> DeleteObject(long id)
    {
        return await _RDB.GeoRemoveAsync("objects", id);
    }

    public async Task<ObjectInfo[]> GetObjectsInZone(CenterModel center, double radius)
    {
        var radiusResults = _RDB.GeoRadius("objects", ConvertFromShortToDouble(center.X), ConvertFromShortToDouble(center.Y), radius * 30);

        var objectsInfo = new ObjectInfo[radiusResults.Length];

        RedisKey[] keys = radiusResults.Select(x => (RedisKey)("objects_id_" + x.Member)).ToArray();

        var objectsString = await _RDB.StringGetAsync(keys);

        for (var i = 0; i < objectsString.Length; i++)
        {
            if (objectsString[i].HasValue == false) throw new NullReferenceException();

            var objectInfo = JsonSerializer.Deserialize<ObjectInfo>(objectsString[i]!);

            if (objectInfo is null) throw new NullReferenceException();

            objectsInfo[i] = objectInfo;
        }

        return objectsInfo;
    }
}
