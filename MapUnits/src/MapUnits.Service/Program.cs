using MagicOnion;
using MagicOnion.Server;
using MapUnits.Core;
using MapUnits.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IUnitMovementService, UnitMovementService>();

builder.Services.AddGrpc();
builder.Services.AddMagicOnion();

var app = builder.Build();

app.MapMagicOnionService();

//Для добавления юнитов на старте пример

//var movementService = app.Services.GetRequiredService<IUnitMovementService>();

//for (var i = 0; i < 100; i++)
//{
//    movementService.AddUnit(new Unit($"unit_{i}", i * 2, i * 3, 5.0f));
//}

app.Run();

