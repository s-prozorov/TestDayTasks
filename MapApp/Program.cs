using MagicOnion.Serialization.MemoryPack;
using MagicOnion.Serialization;
using StackExchange.Redis;
using Application.Features.Objects.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect("localhost"));

builder.Services.AddScoped<ObjectsService>();

MagicOnionSerializerProvider.Default = MemoryPackMagicOnionSerializerProvider.Instance;

builder.Services.AddMagicOnion();

var app = builder.Build();

app.MapMagicOnionService();

app.Run();