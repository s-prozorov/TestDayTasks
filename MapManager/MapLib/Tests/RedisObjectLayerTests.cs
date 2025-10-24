using FluentAssertions;
using MapLib.Domain;
using MapLib.Engine;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace MapLib.Tests;

/// <summary>
/// Тесты для RedisObjectLayer:
/// проверяют корректность работы операций добавления, удаления, изменения, перемещения,
/// генерацию событий и выборку по идентификатору, координатам, областям.
/// </summary>
public class RedisObjectLayerTests
{
	private static (RedisObjectLayer layer, Mock<IConnectionMultiplexer> multiplexer, Mock<IDatabase> database, 
		Mock<IGridGeoConverter> converter, Mock<ITransaction> transaction, Mock<IBatch> batch) CreateSut()
	{
		var multiplexer = new Mock<IConnectionMultiplexer>(MockBehavior.Strict);
		var database = new Mock<IDatabase>(MockBehavior.Strict);
		var converter = new Mock<IGridGeoConverter>(MockBehavior.Strict);
		var transaction = new Mock<ITransaction>(MockBehavior.Strict);
		var batch = new Mock<IBatch>(MockBehavior.Strict);

		multiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(database.Object);

		database.Setup(d => d.CreateTransaction(null)).Returns(transaction.Object);
		transaction.Setup(t => t.Execute(It.IsAny<CommandFlags>())).Returns(true);

		database.Setup(d => d.CreateBatch(null)).Returns(batch.Object);
		batch.Setup(b => b.Execute()).Verifiable();

		converter.Setup(c => c.ToGeo(It.IsAny<double>(), It.IsAny<double>())).Returns((0.0, 0.0));
		converter.Setup(c => c.TilesToMeters(It.IsAny<double>())).Returns<double>(x => x * 10);

		var sut = new RedisObjectLayer(multiplexer.Object, converter.Object, "obj:geo", "obj:");
		return (sut, multiplexer, database, converter, transaction, batch);
	}

	private static HashEntry[] ObjHash(MapObject mapObject) =>
		new[]
		{
			new HashEntry("id", mapObject.Id.Value),
			new HashEntry("x", mapObject.X),
			new HashEntry("y", mapObject.Y),
			new HashEntry("width", mapObject.Width),
			new HashEntry("height", mapObject.Height)
		};

	[Fact]
	public void Add_Should_Write_Hash_And_Geo_And_Raise_Created()
	{
		var (sut, _, database, converter, transaction, _) = CreateSut();

		var obj = new MapObject
		{
		    Id = new ObjectId("o1"),
		    X = 10,
		    Y = 20,
		    Width = 3,
		    Height = 2
		};

		converter.Setup(c => c.ToGeo(obj.X + obj.Width / 2.0, obj.Y + obj.Height / 2.0)).Returns((1.1, 2.2));
		transaction.Setup(t => t.HashSetAsync("obj:o1", It.IsAny<HashEntry[]>(), It.IsAny<CommandFlags>())).Returns(Task.FromResult(true));
		transaction.Setup(t => t.GeoAddAsync("obj:geo", 1.1, 2.2, "o1", It.IsAny<CommandFlags>())).Returns(Task.FromResult(true));

		MapObject? created = null;
		sut.Created += o => created = o;

		sut.Add(obj);

		created.Should().NotBeNull();
		created!.Id.Value.Should().Be("o1");
		database.Verify(d => d.CreateTransaction(null), Times.Once);
		transaction.Verify(t => t.HashSetAsync("obj:o1", It.IsAny<HashEntry[]>(), It.IsAny<CommandFlags>()), Times.Once);
		transaction.Verify(t => t.GeoAddAsync("obj:geo", 1.1, 2.2, "o1", It.IsAny<CommandFlags>()), Times.Once);
		transaction.Verify(t => t.Execute(It.IsAny<CommandFlags>()), Times.Once);
		converter.Verify(c => c.ToGeo(obj.X + obj.Width / 2.0, obj.Y + obj.Height / 2.0), Times.Once);
	}

	[Fact]
	public void Remove_Should_Delete_Hash_And_Geo_And_Raise_Deleted()
	{
		var (sut, _, database, _, transaction, _) = CreateSut();

		transaction.Setup(t => t.KeyDeleteAsync("obj:o42", It.IsAny<CommandFlags>())).Returns(Task.FromResult(true));
		transaction.Setup(t => t.SortedSetRemoveAsync("obj:geo", "o42", It.IsAny<CommandFlags>())).Returns(Task.FromResult(true));

		ObjectId? deleted = null;
		sut.Deleted += id => deleted = id;

		sut.Remove(new ObjectId("o42"));

		deleted.Should().NotBeNull();
		deleted!.Value.Value.Should().Be("o42");
		
		database.Verify(d => d.CreateTransaction(null), Times.Once);
		transaction.Verify(t => t.KeyDeleteAsync("obj:o42", It.IsAny<CommandFlags>()), Times.Once);
		transaction.Verify(t => t.SortedSetRemoveAsync("obj:geo", "o42", It.IsAny<CommandFlags>()), Times.Once);
		transaction.Verify(t => t.Execute(It.IsAny<CommandFlags>()), Times.Once);
	}

	[Fact]
	public void Move_Should_Update_Geo_And_Hash_And_Raise_Updated()
	{
		var (sut, _, database, converter, transaction, _) = CreateSut();

		var id = new ObjectId("m1");
		database.Setup(d => d.HashGetAll("obj:m1", It.IsAny<CommandFlags>()))
			.Returns(ObjHash(new MapObject { Id = id, X = 1, Y = 1, Width = 2, Height = 2 }));

		converter.Setup(c => c.ToGeo(100 + 2 / 2.0, 200 + 2 / 2.0)).Returns((5.5, 6.6));

		transaction.Setup(t => t.HashSetAsync("obj:m1", It.IsAny<HashEntry[]>(), It.IsAny<CommandFlags>())).Returns(Task.FromResult(true));
		transaction.Setup(t => t.GeoAddAsync("obj:geo", 5.5, 6.6, "m1", It.IsAny<CommandFlags>())).Returns(Task.FromResult(true));

		MapObject? updated = null;
		sut.Updated += o => updated = o;

		sut.Move(id, 100, 200);

		updated.Should().NotBeNull();
		updated!.X.Should().Be(100);
		updated!.Y.Should().Be(200);
		
		database.Verify(d => d.HashGetAll("obj:m1", It.IsAny<CommandFlags>()), Times.Once);
		database.Verify(d => d.CreateTransaction(null), Times.Once);

		transaction.Verify(t => t.HashSetAsync("obj:m1", It.IsAny<HashEntry[]>(), It.IsAny<CommandFlags>()), Times.Once);
		transaction.Verify(t => t.GeoAddAsync("obj:geo", 5.5, 6.6, "m1", It.IsAny<CommandFlags>()), Times.Once);
		transaction.Verify(t => t.Execute(It.IsAny<CommandFlags>()), Times.Once);

		converter.Verify(c => c.ToGeo(100 + 2/2.0, 200 + 2/2.0), Times.Once);
	}

	[Fact]
	public void Resize_Should_Update_Geo_And_Hash_And_Raise_Updated()
	{
		var (sut, _, database, converter, transaction, _) = CreateSut();

		var id = new ObjectId("r1");
		database.Setup(d => d.HashGetAll("obj:r1", It.IsAny<CommandFlags>())).Returns(ObjHash(new MapObject { Id = id, X = 10, Y = 20, Width = 2, Height = 2 }));

		converter.Setup(c => c.ToGeo(10 + 5 / 2.0, 20 + 7 / 2.0)).Returns((7.7, 8.8));

		transaction.Setup(t => t.HashSetAsync("obj:r1", It.IsAny<HashEntry[]>(), It.IsAny<CommandFlags>())).Returns(Task.FromResult(true));
		transaction.Setup(t => t.GeoAddAsync("obj:geo", 7.7, 8.8, "r1", It.IsAny<CommandFlags>())).Returns(Task.FromResult(true));

		MapObject? updated = null;
		sut.Updated += o => updated = o;

		sut.Resize(id, 5, 7);

		updated.Should().NotBeNull();
		updated.Width.Should().Be(5);
		updated.Height.Should().Be(7);

		database.Verify(d => d.HashGetAll("obj:r1", It.IsAny<CommandFlags>()), Times.Once);
		database.Verify(d => d.CreateTransaction(null), Times.Once);
		transaction.Verify(t => t.HashSetAsync("obj:r1", It.IsAny<HashEntry[]>(), It.IsAny<CommandFlags>()), Times.Once);
		transaction.Verify(t => t.GeoAddAsync("obj:geo", 7.7, 8.8, "r1", It.IsAny<CommandFlags>()), Times.Once());
		transaction.Verify(t => t.Execute(It.IsAny<CommandFlags>()), Times.Once);
		converter.Verify(c => c.ToGeo(10 + 5 / 2.0, 20 + 7 / 2.0), Times.Once);
	}

	[Fact]
	public void GetById_Should_Return_Object_When_Hash_Exists()
	{
		var (sut, _, database, _, _, _) = CreateSut();
		var id = new ObjectId("a1");
		database.Setup(d => d.HashGetAll("obj:a1", It.IsAny<CommandFlags>())).Returns(ObjHash(new MapObject { Id = id, X = 3, Y = 4, Width = 2, Height = 1 }));

		var obj = sut.GetById(id);

		obj.Should().NotBeNull();
		obj.X.Should().Be(3);
		obj.Y.Should().Be(4);
		obj.Width.Should().Be(2);
		obj.Height.Should().Be(1);
		
		database.Verify(d => d.HashGetAll("obj:a1", It.IsAny<CommandFlags>()), Times.Once());
	}

	[Fact]
	public void GetById_Should_Return_Null_When_Hash_Is_Empty()
	{
		var (sut, _, database, _, _, _) = CreateSut();
		var id = new ObjectId("none");
		database.Setup(d => d.HashGetAll("obj:none", It.IsAny<CommandFlags>())).Returns(Array.Empty<HashEntry>());

		sut.GetById(id).Should().BeNull();
		
		database.Verify(d => d.HashGetAll("obj:none", It.IsAny<CommandFlags>()), Times.Once);
	}

	[Fact]
	public void Intersects_Should_Return_True_When_Object_Overlaps_Area()
	{
		var (sut, _, database, _, _, _) = CreateSut();

		database.Setup(d => d.HashGetAll("obj:i1", It.IsAny<CommandFlags>())).Returns(ObjHash(new MapObject { Id = new ObjectId("i1"), X = 5, Y = 5, Width = 3, Height = 2 }));
		sut.Intersects(new ObjectId("i1"), 6, 6, 10, 10).Should().BeTrue();
		
		database.Verify(d => d.HashGetAll("obj:i1", It.IsAny<CommandFlags>()), Times.Once);
	}

	[Fact]
	public void Intersects_Should_Return_False_When_Object_Does_Not_Overlap()
	{
		var (sut, _, database, _, _, _) = CreateSut();

		database.Setup(d => d.HashGetAll("obj:i2", It.IsAny<CommandFlags>())).Returns(ObjHash(new MapObject { Id = new ObjectId("i2"), X = 0, Y = 0, Width = 2, Height = 2 }));
		
		sut.Intersects(new ObjectId("i2"), 5, 5, 2, 2).Should().BeFalse();
		database.Verify(d => d.HashGetAll("obj:i2", It.IsAny<CommandFlags>()), Times.Once);
	}
}
