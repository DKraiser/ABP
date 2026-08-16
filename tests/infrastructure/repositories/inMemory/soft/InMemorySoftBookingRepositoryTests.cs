using NUnit;
using ABP.Domain.Entities;
using ABP.Infrastructure.Repositories.InMemory.Soft;

namespace ABP.Tests.Infrastructure.Repositories.InMemory.Soft;

[TestFixture]
public class InMemorySoftBookingRepositoryTests {
    InMemorySoftBookingRepository repository;
    Booking booking;
    Room room;

    [SetUp]
    public async Task SetUp() {
        room = new ("A", 50, 2000, []);
        booking = new (room, new DateTime(2000, 1, 1, 8, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        repository = new ();
        await repository.AddAsync(booking);
    }

    [Test] 
    public async Task FindBooking_Existing() {
        var foundBooking = await repository.FindByIdAsync(booking.Id);
        Assert.That(booking, Is.EqualTo(foundBooking));
        Assert.That(() => booking == foundBooking, Is.False);
    }

    [Test] 
    public async Task FindBooking_NotExisting() {
        var foundBooking = await repository.FindByIdAsync("Some string id");
        Assert.That(foundBooking, Is.Null);
    }

    [Test]
    public async Task AddBooking_New() { 
        var newBooking = new Booking(room, new DateTime(2000, 1, 1, 14, 0, 0), new DateTime(2000, 1, 1, 18, 0, 0), []);
        Assert.That(await repository.FindByIdAsync(newBooking.Id), Is.Null);

        await repository.AddAsync(newBooking);
        var addedBookingCopy = await repository.FindByIdAsync(newBooking.Id);
        Assert.That(addedBookingCopy, Is.EqualTo(newBooking));
        Assert.That(() => addedBookingCopy == newBooking, Is.False);
    }

    [Test]
    public async Task AddBooking_Duplicate() { 
        var oldCount = (await repository.GetAllAsync()).ToList().Count;
        var duplicate = new Booking(booking);
        await repository.AddAsync(duplicate);
        var newCount = (await repository.GetAllAsync()).ToList().Count;

        Assert.That(oldCount, Is.EqualTo(newCount));
    }

    [Test]
    public async Task RemoveBooking_Existing() {
        var oldCount = (await repository.GetAllAsync()).ToList().Count;
        await repository.RemoveAsync(booking.Id);
        var newCount = (await repository.GetAllAsync()).ToList().Count;

        Assert.Multiple(async () => {
            Assert.That(oldCount, Is.EqualTo(newCount + 1));
            Assert.That(await repository.FindByIdAsync(booking.Id), Is.Null);
        });
    }

    [Test]
    public async Task RemoveBooking_NotExisting() {
        var oldCount = (await repository.GetAllAsync()).ToList().Count;
        await repository.RemoveAsync("Some string id");
        var newCount = (await repository.GetAllAsync()).ToList().Count;

        Assert.That(oldCount, Is.EqualTo(newCount));
    }

    [Test]
    public async Task UpdateBooking_Existing() {
        var updatedBooking = await repository.FindByIdAsync(booking.Id);

        updatedBooking?.StartTime = updatedBooking.StartTime.AddHours(1);

        await repository.UpdateAsync(updatedBooking);

        Assert.Multiple(async () => { 
            var foundUpdatedBooking = await repository.FindByIdAsync(booking.Id);
            Assert.That(updatedBooking.Id == booking.Id && 
                booking.Id == foundUpdatedBooking?.Id, Is.True);
            Assert.That(updatedBooking == foundUpdatedBooking, Is.False);
            Assert.That(updatedBooking, Is.EqualTo(foundUpdatedBooking));
            Assert.That(booking, Is.Not.EqualTo(foundUpdatedBooking));
        });
    }

    [Test]
    public async Task UpdateBooking_NotExisting() {
        var newBooking = new Booking(room, new DateTime(2000, 1, 1, 14, 0, 0), new DateTime(2000, 1, 1, 18, 0, 0), []);
        Assert.DoesNotThrowAsync(async () => await repository.UpdateAsync(newBooking));
    }

    [Test]
    public async Task GetAllBookings() { 
        var all = await repository.GetAllAsync();
        Assert.Multiple(async () => {
            Assert.That(all.ToList(), Has.Count.EqualTo(1));
            Assert.That(all.ToList()[0], Is.EqualTo(booking));
        });
    }

    [Test]
    public async Task FindByDateTime() {
        var booking1 = new Booking(room, new DateTime(2000, 1, 1, 12, 0, 0), new DateTime(2000, 1, 1, 13, 0, 0), []);
        var booking2 = new Booking(room, new DateTime(2000, 1, 1, 14, 0, 0), new DateTime(2000, 1, 1, 18, 0, 0), []);
        var booking3 = new Booking(room, new DateTime(2000, 1, 1, 0, 0, 0), new DateTime(2000, 1, 1, 10, 0, 0), []);
        var booking4 = new Booking(room, new DateTime(2000, 1, 1, 0, 0, 0), new DateTime(2000, 1, 1, 14, 0, 0), []);
        await repository.AddAsync(booking1);
        await repository.AddAsync(booking2);
        await repository.AddAsync(booking3);
        await repository.AddAsync(booking4);

        var relevant = (await repository.FindByDateTimeAsync
            (new DateTime(2000, 1, 1, 2, 0, 0), new DateTime(2000, 1, 1, 13, 30, 0))).ToList();
                
        Assert.Multiple(() => {
            Assert.That(relevant, Has.Count.EqualTo(2));
            Assert.That(relevant, Contains.Item(booking));
            Assert.That(relevant, Contains.Item(booking1));
        });
    }
}