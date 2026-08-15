using NUnit;
using ABP.Domain.Entities;
using ABP.Infrastructure.InMemory.Repositories;

namespace ABP.Tests.Infrastructure.InMemory.Repositories;

[TestFixture]
public class InMemoryRoomRepositoryTests {
    Room room;
    InMemoryRoomRepository repository;

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
        var oldCount = (await repository.GetAllAsync()).ToList().Count;
        var duplicate = new Room(room);
        await repository.AddAsync(duplicate);
        var newCount = (await repository.GetAllAsync()).ToList().Count;

        Assert.That(oldCount, Is.EqualTo(newCount));
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
        var oldCount = (await repository.GetAllAsync()).ToList().Count;
        await repository.RemoveAsync("Some string id");
        var newCount = (await repository.GetAllAsync()).ToList().Count;

        Assert.That(oldCount, Is.EqualTo(newCount));
    }

    [Test]
    public async Task UpdateRoom_Existing() {
        var updatedRoom = await repository.FindByIdAsync(room.Id);

        updatedRoom?.BasePrice = 3000;
        updatedRoom?.Services.Add(new Service ("Internet", 500));

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
        Assert.That(async () => await repository.UpdateAsync(newRoom), Throws.InvalidOperationException);
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