using MapLib.Core.Interfaces;
using MapLib.Object.Redis;
using MapLib.Services;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MapLib;

public static class MapLibExtensions
{
    public static IServiceCollection AddMapLib(this IServiceCollection services, string redisConnectionString)
    {
        services.AddSingleton<MapProvider>();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        
        services.AddSingleton<DataContext>();

        services.AddScoped<IMapService, MapService>();
        services.AddScoped<IObjectService, ObjectService>();
        services.AddScoped<IGameService, GameService>();

        return services;
    }
}