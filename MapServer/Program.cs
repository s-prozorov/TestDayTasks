using MagicOnion.Serialization;
using MagicOnion.Serialization.MemoryPack;
using MapLib;
using MapLib.Interfaces;
using MapService.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ObjectsLib;
using ObjectsLib.Interfaces;
using StackExchange.Redis;

Console.WriteLine("MapServer starting...");

MagicOnionSerializerProvider.Default = MemoryPackMagicOnionSerializerProvider.Instance;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("SERVER_PORT");
    var serverPort = port != null ? int.Parse(port) : 5001;

    options.ListenAnyIP(serverPort, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379";
Console.WriteLine($"Connecting to Redis: {redisConnectionString}");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

builder.Services.AddSingleton<IMap>(sp =>
{
    var generator = new MapGenerator(seed: 12345);
    var map = generator.Generate();
    Console.WriteLine("Map generated with 87 territories");
    return map;
});

builder.Services.AddSingleton<IObjectStorage, ObjectStorage>();

builder.Services.AddSingleton<IMapEventBroadcaster, MapService.MapEventBroadcaster>();
builder.Services.AddSingleton<MapService.MapGroupManager>();
builder.Services.AddHostedService<MapService.MapEventBroadcastService>();

builder.Services.AddSingleton<MapService.Interfaces.IMapService, MapService.MapClientService>();

builder.Services.AddGrpc();
builder.Services.AddMagicOnion();

var app = builder.Build();

app.MapMagicOnionService();

var serverUrl = Environment.GetEnvironmentVariable("SERVER_URL") ?? "http://localhost:5000";
Console.WriteLine($"MapServer listening on {serverUrl}");
Console.WriteLine("Services registered: MapClientService, MapHub");
Console.WriteLine("Press Ctrl+C to shutdown");
Console.WriteLine();

await app.RunAsync();
