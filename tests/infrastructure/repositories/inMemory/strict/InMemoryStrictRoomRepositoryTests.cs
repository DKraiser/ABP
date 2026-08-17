using NUnit;
using ABP.Domain.Entities;
using ABP.Infrastructure.Repositories.InMemory.Strict;
using ABP.Application.Exceptions;

namespace ABP.Tests.Infrastructure.Repositories.InMemory.Strict;

[TestFixture]
public class InMemoryStrictRoomRepositoryTests {
    InMemoryStrictRoomRepository repository;
    Room room;

    [SetUp]
    public async Task SetUp() {
        room = new ("A", 50, 2000, []);
        repository = new ();
        await repository.AddAsync(room);
    }

    [Test] 
    public async Task FindRoom_Existing() {
        var foundRoom = await repository.FindByIdAsync(room.Id);
        Assert.That(room, Is.EqualTo(foundRoom));
        Assert.That(() => room == foundRoom, Is.False);
    }

    [Test] 
    public async Task FindRoom_NotExisting() {
        var foundRoom = await repository.FindByIdAsync("Some string id");
        Assert.That(foundRoom, Is.Null);
    }

    [Test]
    public async Task AddRoom_New() { 
        var service = new Service ("Internet", 500);
        var newRoom = new Room ("B", 100, 3000, [ service ]);
        Assert.That(await repository.FindByIdAsync(newRoom.Id), Is.Null);

        await repository.AddAsync(newRoom);
        var addedRoomCopy = await repository.FindByIdAsync(newRoom.Id);
        Assert.That(addedRoomCopy, Is.EqualTo(newRoom));
        Assert.That(() => addedRoomCopy == newRoom, Is.False);
    }

    [Test]
    public async Task AddRoom_Duplicate() { 
        var duplicate = new Room(room);
        Assert.ThrowsAsync<RepositoryException>(async () => await repository.AddAsync(duplicate));
    }

    [Test]
    public async Task RemoveRoom_Existing() {
        var oldCount = (await repository.GetAllAsync()).ToList().Count;
        await repository.RemoveAsync(room.Id);
        var newCount = (await repository.GetAllAsync()).ToList().Count;

        Assert.Multiple(async () => {
            Assert.That(oldCount, Is.EqualTo(newCount + 1));
            Assert.That(await repository.FindByIdAsync(room.Id), Is.Null);
        });
    }

    [Test]
    public async Task RemoveRoom_NotExisting() {
        Assert.ThrowsAsync<RepositoryException>(async () => await repository.RemoveAsync("Some string id"));
    }

    [Test]
    public async Task UpdateRoom_Existing() {
        var updatedRoom = await repository.FindByIdAsync(room.Id);

        updatedRoom?.BasePrice = 3000;
        updatedRoom?.AddService(new Service ("Internet", 500));

        await repository.UpdateAsync(updatedRoom);

        Assert.Multiple(async () => { 
            var foundUpdatedRoom = await repository.FindByIdAsync(room.Id);
            Assert.That(updatedRoom.Id == room.Id && 
                room.Id == foundUpdatedRoom?.Id, Is.True);
            Assert.That(updatedRoom == foundUpdatedRoom, Is.False);
            Assert.That(updatedRoom, Is.EqualTo(foundUpdatedRoom));
            Assert.That(room, Is.Not.EqualTo(foundUpdatedRoom));
        });
    }

    [Test]
    public async Task UpdateRoom_NotExisting() {
        var service = new Service ("Internet", 500);
        var newRoom = new Room ("B", 100, 3000, [ service ]);
        Assert.ThrowsAsync<RepositoryException>(async () => await repository.UpdateAsync(newRoom));
    }

    [Test]
    public async Task GetAllRooms() { 
        var all = await repository.GetAllAsync();
        Assert.Multiple(async () => {
            Assert.That(all.ToList(), Has.Count.EqualTo(1));
            Assert.That(all.ToList()[0], Is.EqualTo(room));
        });
    }
}