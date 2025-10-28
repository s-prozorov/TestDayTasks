using MagicOnion;
using MagicOnion.Server;
using MapNetworkService.Services;
using MapObjects;
using MapRegions;
using MapSurface;
using MapUnits.Core;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
var mapWidth = builder.Configuration.GetValue<int>("Map:Width");
var mapHeight = builder.Configuration.GetValue<int>("Map:Height");
var regionCount = builder.Configuration.GetValue<int>("Map:RegionCount");

builder.Services.AddSingleton<ISurfaceLayer>(sp =>
{
    var surface = new SurfaceLayer(mapWidth, mapHeight);
    return surface;
});

builder.Services.AddSingleton<IMapObjectLayer>(sp => 
    new MapObjectLayer(redisConnectionString));

builder.Services.AddSingleton<IRegionLayer>(sp =>
{
    var generator = new VoronoiRegionGenerator(42);
    return RegionLayer.Create(
        mapWidth,
        mapHeight,
        regionCount,
        generator,
        id => $"Region {id}");
});

builder.Services.AddSingleton<IUnitMovementService>(sp =>
{
    var surfaceLayer = sp.GetRequiredService<ISurfaceLayer>();
    return new UnitMovementService(100, 32, surfaceLayer);
});

builder.Services.AddGrpc();
builder.Services.AddMagicOnion();

var app = builder.Build();

app.MapMagicOnionService();

app.Run();

