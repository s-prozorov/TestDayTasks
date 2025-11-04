using MapLib;
using MapLib.Core.Interfaces;
using MapLib.Core.Models.ObjectModel;
using MapService.Interfaces;
using MapService.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
    options.ListenLocalhost(5001, listenOptions =>
    {
        listenOptions.Protocols =  HttpProtocols.Http1AndHttp2;
    });
});

builder.Services.AddMapLib("localhost:6379");

builder.Services.AddMagicOnion();
builder.Services.AddScoped<IMapHub, MapHubService>();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapMagicOnionService<MapHubService>();
app.MapGet("/", () =>
{
});


app.Run();
public partial class Program { }
