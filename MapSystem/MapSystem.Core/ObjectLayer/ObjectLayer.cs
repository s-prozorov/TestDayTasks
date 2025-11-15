using StackExchange.Redis;

namespace MapSystem.Core.ObjectLayer;

/// <summary>
///     Фабрика для создания различных реализаций IObjectLayer.
/// </summary>
public static class ObjectLayer
{
    /// <summary>
    ///     Создает ObjectLayer с использованием Redis для хранения.
    /// </summary>
    public static IObjectLayer Create(IConnectionMultiplexer redis, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(redis, nameof(redis));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width, nameof(width));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height, nameof(height));

        return new RedisObjectLayer(redis, "map");
    }

    /// <summary>
    ///     Создает ObjectLayer с использованием in-memory хранилища.
    /// </summary>
    public static IObjectLayer CreateInMemory(int width, int height)
    {
        return new InMemoryObjectLayer(width, height);
    }
}
