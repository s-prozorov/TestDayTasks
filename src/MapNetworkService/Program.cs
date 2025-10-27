using MagicOnion;
using MagicOnion.Server;
using MapNetworkService.Services;
using MapObjects;
using MapRegions;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
var mapWidth = builder.Configuration.GetValue<int>("Map:Width");
var mapHeight = builder.Configuration.GetValue<int>("Map:Height");
var regionCount = builder.Configuration.GetValue<int>("Map:RegionCount");

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

builder.Services.AddGrpc();
builder.Services.AddMagicOnion();

var app = builder.Build();

app.MapMagicOnionService();

app.Run();

