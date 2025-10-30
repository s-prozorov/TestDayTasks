using Moq;
using ObjectsLib;
using ObjectsLib.Models;
using StackExchange.Redis;

namespace UnitTests;

public class ObjectStorageUnitTests
{
    private readonly Mock<IConnectionMultiplexer> _connectionMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly ObjectStorage _storage;

    public ObjectStorageUnitTests()
    {
        _connectionMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        
        _connectionMock.Setup(x => 
                x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);
        
        _storage = new ObjectStorage(_connectionMock.Object);
    }

    [Fact]
    public async Task CreateObjectAsync_ValidObject_CallsRedisCorrectly()
    {
        // Arrange
        var obj = new ObjectData
        {
            Id = "test-1",
            X = 100,
            Y = 200,
            Type = ObjectType.PlayerBase
        };

        var transactionMock = new Mock<ITransaction>();
        transactionMock.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        transactionMock.Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
                .Returns(Task.FromResult(true));
        
        transactionMock.Setup(x => x.GeoAddAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .Returns(Task.FromResult(true));

        _databaseMock.Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Returns(transactionMock.Object);

        // Act
        var result = await _storage.CreateObjectAsync(obj);

        // Assert
        Assert.Equal(obj.Id, result.Id);
        // Проверяем что транзакция выполнилась
        transactionMock.Verify(x => x.ExecuteAsync(It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task CreateObjectAsync_NullObject_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _storage.CreateObjectAsync(null!));
    }

    [Fact]
    public async Task CreateObjectAsync_EmptyId_ThrowsArgumentException()
    {
        // Arrange
        var obj = new ObjectData
        {
            Id = "",
            X = 100,
            Y = 200,
            Type = ObjectType.Mine
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _storage.CreateObjectAsync(obj));
    }

    [Fact]
    public async Task CreateObjectAsync_TransactionFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var obj = new ObjectData
        {
            Id = "test-fail",
            X = 100,
            Y = 200,
            Type = ObjectType.PlayerBase
        };

        var transactionMock = new Mock<ITransaction>();
        transactionMock.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>()))
            .ReturnsAsync(false); // Транзакция не прошла

        _databaseMock.Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Returns(transactionMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _storage.CreateObjectAsync(obj));
    }

    [Fact]
    public async Task GetObjectAsync_ExistingObject_ReturnsDeserializedObject()
    {
        // Arrange
        var json = """{"Id":"test-1","X":100.0,"Y":200.0,"Type":0,"OwnerId":5,"CreatedAt":"2024-01-01T00:00:00Z"}""";
        
        _databaseMock.Setup(x => x.StringGetAsync(
            It.Is<RedisKey>(k => k.ToString().Contains("test-1")),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(json);

        // Act
        var result = await _storage.GetObjectAsync("test-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-1", result.Id);
        Assert.Equal(100, result.X);
        Assert.Equal(200, result.Y);
    }

    [Fact]
    public async Task GetObjectAsync_NonExistingObject_ReturnsNull()
    {
        // Arrange
        _databaseMock.Setup(x => x.StringGetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _storage.GetObjectAsync("non-existing");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetObjectAsync_EmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _storage.GetObjectAsync(""));
    }

    [Fact]
    public async Task DeleteObjectAsync_CallsRedisCorrectly()
    {
        // Arrange
        var transactionMock = new Mock<ITransaction>();
        transactionMock.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        transactionMock.Setup(x => x.GeoRemoveAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        transactionMock.Setup(x => x.KeyDeleteAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Returns(transactionMock.Object);

        // Act
        var result = await _storage.DeleteObjectAsync("test-1");

        // Assert
        Assert.True(result);
        transactionMock.Verify(x => x.GeoRemoveAsync(
            It.IsAny<RedisKey>(),
            It.Is<RedisValue>(v => v == "test-1"),
            It.IsAny<CommandFlags>()), Times.Once);
        transactionMock.Verify(x => x.KeyDeleteAsync(
            It.Is<RedisKey>(k => k.ToString().Contains("test-1")),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task DeleteObjectAsync_EmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _storage.DeleteObjectAsync(""));
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        // Arrange
        _databaseMock.Setup(x => x.KeyExistsAsync(
            It.Is<RedisKey>(k => k.ToString().Contains("test-1")),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _storage.ExistsAsync("test-1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingKey_ReturnsFalse()
    {
        // Arrange
        _databaseMock.Setup(x => x.KeyExistsAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var result = await _storage.ExistsAsync("non-existing");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetObjectsInZoneAsync_NegativeRadius_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _storage.GetObjectsInZoneAsync(100, 100, -10));
    }

    [Fact]
    public async Task GetObjectsInZoneAsync_ValidRequest_ReturnsObjects()
    {
        // Arrange
        var geoResults = new[]
        {
            new GeoRadiusResult((RedisValue)"obj-1", null, null, null),
            new GeoRadiusResult((RedisValue)"obj-2", null, null, null)
        };

        _databaseMock.Setup(x => x.GeoRadiusAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<GeoUnit>(),
            It.IsAny<int>(),
            It.IsAny<Order?>(),
            It.IsAny<GeoRadiusOptions>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(geoResults);

        var json1 = """{"Id":"obj-1","X":100.0,"Y":100.0,"Type":0,"OwnerId":null,"CreatedAt":"2024-01-01T00:00:00Z"}""";
        var json2 = """{"Id":"obj-2","X":105.0,"Y":105.0,"Type":1,"OwnerId":null,"CreatedAt":"2024-01-01T00:00:00Z"}""";

        _databaseMock.Setup(x => x.StringGetAsync(
            It.IsAny<RedisKey[]>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(new[] { (RedisValue)json1, (RedisValue)json2 });

        // Act
        var results = await _storage.GetObjectsInZoneAsync(100, 100, 50);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("obj-1", results[0].Id);
        Assert.Equal("obj-2", results[1].Id);
    }
}