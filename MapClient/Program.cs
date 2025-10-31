using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization;
using MagicOnion.Serialization.MemoryPack;
using MapService.Events;
using MapService.Interfaces;
using MapService.Requests;
using ObjectsLib.Models;

Console.WriteLine("=== MapClient ===");
Console.WriteLine("Connecting to server...");

// Настройка MemoryPack сериализации для MagicOnion
MagicOnionSerializerProvider.Default = MemoryPackMagicOnionSerializerProvider.Instance;

// Создание gRPC канала
var serverAddress = Environment.GetEnvironmentVariable("SERVER_ADDRESS") ?? "http://localhost:5000";
Console.WriteLine($"Server address: {serverAddress}");
var channel = GrpcChannel.ForAddress(serverAddress);

// Создание MagicOnion клиента
var client = MagicOnion.Client.MagicOnionClient.Create<IMapService>(channel);

// Создание получателя StreamingHub для событий изменения
var receiver = new MapHubReceiver();
var hub = await StreamingHubClient.ConnectAsync<IMapHub, IMapHubReceiver>(channel, receiver);

Console.WriteLine("Connected to MapServer!");
Console.WriteLine();
Console.WriteLine("Available commands:");
Console.WriteLine("  create <type> <x> <y>    - Create object (type: 0=PlayerBase, 1=Mine, 2=Temporary)");
Console.WriteLine("  delete <id>              - Delete object");
Console.WriteLine("  move <id> <x> <y>        - Move object");
Console.WriteLine("  state <x1> <y1> <x2> <y2> - Get objects in area");
Console.WriteLine("  regions <x1> <y1> <x2> <y2> - Get regions in area");
Console.WriteLine("  help                     - Show this help");
Console.WriteLine("  exit                     - Exit client");
Console.WriteLine();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) continue;

    var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0) continue;

    var command = parts[0].ToLower();

    try
    {
        switch (command)
        {
            case "create":
                if (parts.Length != 4)
                {
                    Console.WriteLine("Usage: create <type> <x> <y>");
                    break;
                }
                await HandleCreate(client, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                break;

            case "delete":
                if (parts.Length != 2)
                {
                    Console.WriteLine("Usage: delete <id>");
                    break;
                }
                await HandleDelete(client, parts[1]);
                break;

            case "move":
                if (parts.Length != 4)
                {
                    Console.WriteLine("Usage: move <id> <x> <y>");
                    break;
                }
                await HandleMove(client, parts[1], int.Parse(parts[2]), int.Parse(parts[3]));
                break;

            case "state":
                if (parts.Length != 5)
                {
                    Console.WriteLine("Usage: state <x1> <y1> <x2> <y2>");
                    break;
                }
                await HandleState(client, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]));
                break;

            case "regions":
                if (parts.Length != 5)
                {
                    Console.WriteLine("Usage: regions <x1> <y1> <x2> <y2>");
                    break;
                }
                await HandleRegions(client, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]));
                break;

            case "help":
                Console.WriteLine("Available commands:");
                Console.WriteLine("  create <type> <x> <y>    - Create object");
                Console.WriteLine("  delete <id>              - Delete object");
                Console.WriteLine("  move <id> <x> <y>        - Move object");
                Console.WriteLine("  state <x1> <y1> <x2> <y2> - Get objects in area");
                Console.WriteLine("  regions <x1> <y1> <x2> <y2> - Get regions in area");
                Console.WriteLine("  exit                     - Exit client");
                break;

            case "exit":
                Console.WriteLine("Disconnecting...");
                await hub.DisposeAsync();
                await channel.ShutdownAsync();
                return;

            default:
                Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex.Message}");
    }
}

static async Task HandleCreate(IMapService client, int type, int x, int y)
{
    var request = new CreateObjectRequest { ObjectTypeId = type, X = x, Y = y };
    var response = await client.CreateObjectAsync(request);

    if (response.IsSuccess == true)
        Console.WriteLine($"[SUCCESS] Created object: {response.ObjectId} at ({x},{y})");
    else
        Console.WriteLine("[FAILED] Could not create object");
}

static async Task HandleDelete(IMapService client, string id)
{
    var request = new DeleteObjectRequest { ObjectId = id };
    var response = await client.DeleteObjectAsync(request);

    if (response.IsSuccess == true)
        Console.WriteLine($"[SUCCESS] Deleted object: {id}");
    else
        Console.WriteLine($"[FAILED] Could not delete object: {id}");
}

static async Task HandleMove(IMapService client, string id, int x, int y)
{
    var request = new UpdateObjectRequest { ObjectId = id, X = x, Y = y };
    var response = await client.UpdateObjectAsync(request);

    if (response.IsSuccess == true)
        Console.WriteLine($"[SUCCESS] Moved object {id} to ({x},{y})");
    else
        Console.WriteLine($"[FAILED] Could not move object: {id}");
}

static async Task HandleState(IMapService client, int x1, int y1, int x2, int y2)
{
    var request = new GetObjectsInAreaRequest { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 };
    var response = await client.GetObjectsInAreaAsync(request);

    Console.WriteLine($"[INFO] Objects in area ({x1},{y1}) to ({x2},{y2}): {response.Objects.Length} found");
    foreach (var obj in response.Objects)
    {
        var typeName = obj.Type.HasValue ? ((ObjectType)obj.Type.Value).ToString() : "Unknown";
        Console.WriteLine($"  - ID: {obj.Id}, Type: {typeName}, Pos: ({obj.X},{obj.Y}), Owner: {obj.OwnerId}");
    }
}

static async Task HandleRegions(IMapService client, int x1, int y1, int x2, int y2)
{
    var request = new GetRegionsInAreaRequest { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 };
    var response = await client.GetRegionsInAreaAsync(request);

    Console.WriteLine($"[INFO] Regions in area ({x1},{y1}) to ({x2},{y2}): {response.Regions.Length} found");
    foreach (var region in response.Regions)
    {
        Console.WriteLine($"  - ID: {region.Id}, Name: {region.Name}, Owner: {region.OwnerId}");
    }
}

// Получатель StreamingHub для событий изменения
public class MapHubReceiver : IMapHubReceiver
{
    public void OnObjectAdded(ObjectAddedEvent ev)
    {
        Console.WriteLine($"[DELTA] Object Added: {ev.Map.Id} at ({ev.Map.X},{ev.Map.Y})");
    }

    public void OnObjectUpdated(ObjectUpdatedEvent ev)
    {
        Console.WriteLine($"[DELTA] Object Updated: {ev.Map.Id} moved to ({ev.Map.X},{ev.Map.Y})");
    }

    public void OnObjectDeleted(ObjectDeletedEvent ev)
    {
        Console.WriteLine($"[DELTA] Object Deleted: {ev.ObjectId}");
    }
}
