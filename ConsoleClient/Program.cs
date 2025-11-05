using Application.Features.Objects.Enums;
using Application.Features.Objects.Hubs;
using Application.Features.Objects.Models;
using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnion.Serialization;
using MagicOnion.Serialization.MemoryPack;
using System.Text.Json;


var channel = GrpcChannel.ForAddress("http://localhost:5000");

MagicOnionSerializerProvider.Default = MemoryPackMagicOnionSerializerProvider.Instance;

var receiver = new ObjectHubReceiver();

var client = await StreamingHubClient.ConnectAsync<IObjectHub, IObjectHubReceiver>(channel, receiver);

Console.WriteLine("Игра началась!");
Console.WriteLine("Тут ты можешь работать с своими объектами на карте.");

long TryParseLong(string? value, string? message)
{
    if (long.TryParse(value, out long result))
    {
        return result;
    }
    else
    {
        Console.WriteLine(message);
        return TryParseLong(Console.ReadLine(), message);
    }
}

short TryParseShort(string? value, string? message)
{
    if (short.TryParse(value, out short result))
    {
        return result;
    }
    else
    {
        Console.WriteLine(message);
        return TryParseShort(Console.ReadLine(), message);
    }
}

ObjectTypeEnum TryParseObjectTypeEnum(string? value, string? message)
{
    if (Enum.TryParse(typeof(ObjectTypeEnum), value, out var result))
    {
        if (!Enum.IsDefined(typeof(ObjectTypeEnum), result))
        {
            Console.WriteLine("Некорректное значение типа объекта. Попробуйте снова.");
            return TryParseObjectTypeEnum(Console.ReadLine(), message);
        }
        return (ObjectTypeEnum)result;
    }
    else
    {
        Console.WriteLine(message);
        return TryParseObjectTypeEnum(Console.ReadLine(), message);
    }
}

while (true)
{
    Console.WriteLine("Введите номер команды на выбор: 1 - создать объект, 2 - переместить объект, 3 - получить объекты в области, 4 - удалить объект, 5 - выйти.");
    Console.WriteLine("Введите команду: ");

    var comandLine = Console.ReadLine();

    if (comandLine is null)
    {
        Console.WriteLine("Введите команду: ");
        continue;
    }

    if (comandLine == "1")
    {
        Console.WriteLine("Введите X координату");
        var x = TryParseShort(Console.ReadLine(), "Введите корректную координату");

        Console.WriteLine("Введите Y координату");
        var y = TryParseShort(Console.ReadLine(), "Введите корректную координату");

        Console.WriteLine("Введите Type: 1 - Basa, 2 - Mine, 3 - TemporaryObject");
        var type = TryParseObjectTypeEnum(Console.ReadLine(), "Введите корректный тип объекта");

        var id = await client.CreateObject(new ObjectInfo
        {
            X = x,
            Y = y,
            Type = type
        });

        Console.WriteLine("ID созданного объекта: " + id.ToString());
    }
    else if (comandLine == "2")
    {
        Console.WriteLine("Введите ID объекта");
        var id = TryParseLong(Console.ReadLine(), "Введите корректный ID объекта");

        Console.WriteLine("Введите новую X координату");
        var x = TryParseShort(Console.ReadLine(), "Введите корректную координату");

        Console.WriteLine("Введите новую Y координату");
        var y = TryParseShort(Console.ReadLine(), "Введите корректную координату");

        var isMoved = await client.MoveObject(id, x, y);
        if (isMoved)
        {
            Console.WriteLine("Объект перемещен");
        }
        else
        {
            Console.WriteLine("Объект не перемещен");
        }
    }
    else if (comandLine == "3")
    {
        Console.WriteLine("Введите X координату");
        var x1 = TryParseShort(Console.ReadLine(), "Введите корректную координату");

        Console.WriteLine("Введите Y координату");
        var y1 = TryParseShort(Console.ReadLine(), "Введите корректную координату");

        Console.WriteLine("Введите радиус");
        var radius = TryParseShort(Console.ReadLine(), "Введите корректный радиус");

        var objects = await client.GetObjectsInRadiusRequest(x1, y1, radius);

        if (objects.Length == 0)
        {
            Console.WriteLine("Объекты не найдены");
        }
        else
        {
            foreach (var objectInfo in objects)
            {
                Console.WriteLine(JsonSerializer.Serialize(objectInfo));
            }
        }

    }
    else if (comandLine == "4")
    {
        Console.WriteLine("Введите ID объекта");
        var id = TryParseLong(Console.ReadLine(), "Введите корректный ID объекта");
        var isDeleted = await client.DeleteObject(id);
        if (isDeleted)
        {
            Console.WriteLine("Объект удален");
        }
        else
        {
            Console.WriteLine("Объект не удален");
        }
    }
    else if (comandLine == "5")
    {
        await client.DisposeAsync();
        break;
    }
}


class ObjectHubReceiver : IObjectHubReceiver
{
    public void OnObjectAdded(ObjectInfo objectInfo)
    {
        Console.WriteLine("Был добавлен новый объект.");
        Console.WriteLine(JsonSerializer.Serialize(objectInfo));
    }
    public void OnObjectMoved(long id, short x, short y)
    {
        Console.WriteLine(id.ToString() + " объект перемещен на координаты " + x.ToString() + ", " + y.ToString());
    }

    public void OnObjectDeleted(long id)
    {
        Console.WriteLine(id.ToString() + " объект удален");
    }
}