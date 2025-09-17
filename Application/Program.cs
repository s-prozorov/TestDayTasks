using Application.Settings;
using RegionLayerEngine;
using StackExchange.Redis;
using TileMapEngine.Models;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация параметров
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

// Настройки Redis
var redisConfig = builder.Configuration.GetSection("Redis").Get<RedisSettings>();
// Настройки карты
var mapConfig = builder.Configuration.GetSection("Map").Get<MapSettings>();
// Настройки регионов
var regionConfig = builder.Configuration.GetSection("Regions").Get<RegionSettings>();

// Регистрация сервисов
builder.Services.AddSingleton<TileLayer>(_ => 
    new TileLayer(mapConfig.Width, mapConfig.Height));

builder.Services.AddSingleton<RegionLayer>(_ => 
    new RegionLayer(
        mapConfig.Width, 
        mapConfig.Height, 
        regionConfig.Size));

builder.Services.AddSingleton<ObjectLayer>(_ => 
    new ObjectLayer(redisConfig.ConnectionString));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => 
    ConnectionMultiplexer.Connect(redisConfig.ConnectionString));

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();