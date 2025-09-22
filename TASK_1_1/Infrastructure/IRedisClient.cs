namespace TASK_1_1.Infrastructure;
public interface IRedisGeoClient
{
    void GeoAdd(string key, double longitude, double latitude, string member);
    void GeoRemove(string key, string member);
    IEnumerable<string> GeoRadius(string key, double longitude, double latitude, double radius);
}
