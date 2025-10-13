using TASK_1_1.Infrastructure;

namespace TASK_1_1.Tests.Utilities;
public class FakeRedisGeoClient : IRedisGeoClient
{
    private readonly Dictionary<string, Dictionary<string, (double lon, double lat)>> data
        = new();

    public void GeoAdd(string key, double longitude, double latitude, string member)
    {
        if (!data.ContainsKey(key)) data[key] = new Dictionary<string, (double, double)>();
        data[key][member] = (longitude, latitude);
    }

    public void GeoRemove(string key, string member)
    {
        data.TryGetValue(key, out var dict);
        dict?.Remove(member);
    }

    public IEnumerable<string> GeoRadius(string key, double longitude, double latitude, double radius)
    {
        if (!data.ContainsKey(key)) return Enumerable.Empty<string>();

        return data[key]
            .Where(kv => Math.Abs(kv.Value.lon - longitude) <= radius &&
                         Math.Abs(kv.Value.lat - latitude) <= radius)
            .Select(kv => kv.Key);
    }
}
