using ABP.Application.Dto.Commands.SearchRoomsHandler;
using ABP.Application.Implementations.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using Moq;

namespace ABP.Tests.Application.Implementations.Handlers;

[TestFixture]
public class SearchRoomsHandlerTests
{
    private Mock<IBookingRepository> _bookingRepository = null!;
    private Mock<IRoomRepository> _roomRepository = null!;
    private SearchRoomsHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = new Mock<IBookingRepository>();
        _roomRepository = new Mock<IRoomRepository>();

        _handler = new SearchRoomsHandler(
            _bookingRepository.Object,
            _roomRepository.Object);
    }

    [Test]
    public async Task SearchRoomsAsync_ReturnsRoomsWithSufficientCapacity()
    {
        var smallRoom = new Room("Small", 2, 100, []);
        var largeRoom = new Room("Large", 10, 200, []);

        _roomRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([smallRoom, largeRoom]);

        _bookingRepository
            .Setup(r => r.FindByDateTimeAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        var command = new SearchRoomsCommand(
            new DateOnly(2026, 8, 20),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            5);

        var result = await _handler.SearchRoomsAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value[0].Id, Is.EqualTo(largeRoom.Id));
        });
    }

    [Test]
    public async Task SearchRoomsAsync_ExcludesBookedRooms()
    {
        var availableRoom = new Room("Available", 10, 100, []);
        var bookedRoom = new Room("Booked", 10, 200, []);

        var booking = new Booking(
            bookedRoom,
            new DateTime(2026, 8, 20, 10, 0, 0),
            new DateTime(2026, 8, 20, 12, 0, 0),
            []);

        _roomRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([availableRoom, bookedRoom]);

        _bookingRepository
            .Setup(r => r.FindByDateTimeAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync([booking]);

        var command = new SearchRoomsCommand(
            new DateOnly(2026, 8, 20),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            5);

        var result = await _handler.SearchRoomsAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value[0].Id, Is.EqualTo(availableRoom.Id));
        });
    }

    [Test]
    public async Task SearchRoomsAsync_ReturnsEmpty_WhenAllSuitableRoomsAreBooked()
    {
        var room1 = new Room("Room 1", 10, 100, []);
        var room2 = new Room("Room 2", 15, 150, []);

        var booking1 = new Booking(
            room1,
            new DateTime(2026, 8, 20, 10, 0, 0),
            new DateTime(2026, 8, 20, 12, 0, 0),
            []);

        var booking2 = new Booking(
            room2,
            new DateTime(2026, 8, 20, 10, 0, 0),
            new DateTime(2026, 8, 20, 12, 0, 0),
            []);

        _roomRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([room1, room2]);

        _bookingRepository
            .Setup(r => r.FindByDateTimeAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync([booking1, booking2]);

        var command = new SearchRoomsCommand(
            new DateOnly(2026, 8, 20),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            5);

        var result = await _handler.SearchRoomsAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }

    [Test]
    public async Task SearchRoomsAsync_ReturnsEmpty_WhenNoRoomsHaveSufficientCapacity()
    {
        var room1 = new Room("Room 1", 2, 100, []);
        var room2 = new Room("Room 2", 3, 150, []);

        _roomRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([room1, room2]);

        _bookingRepository
            .Setup(r => r.FindByDateTimeAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        var command = new SearchRoomsCommand(
            new DateOnly(2026, 8, 20),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            5);

        var result = await _handler.SearchRoomsAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }

    [Test]
    public async Task SearchRoomsAsync_IncludesRoom_WhenCapacityExactlyMatchesMinimum()
    {
        var room = new Room("Room", 5, 100, []);

        _roomRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([room]);

        _bookingRepository
            .Setup(r => r.FindByDateTimeAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        var command = new SearchRoomsCommand(
            new DateOnly(2026, 8, 20),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            5);

        var result = await _handler.SearchRoomsAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value[0].Id, Is.EqualTo(room.Id));
        });
    }

    [Test]
    public async Task SearchRoomsAsync_MapsRoomToRoomInfo()
    {
        var service = new Service("Projector", 20);
        var room = new Room("Room A", 10, 100, [service]);

        _roomRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([room]);

        _bookingRepository
            .Setup(r => r.FindByDateTimeAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        var command = new SearchRoomsCommand(
            new DateOnly(2026, 8, 20),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            5);

        var result = await _handler.SearchRoomsAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);

            var info = result.Value[0];

            Assert.That(info.Id, Is.EqualTo(room.Id));
            Assert.That(info.Name, Is.EqualTo(room.Name));
            Assert.That(info.Capacity, Is.EqualTo(room.Capacity));
            Assert.That(info.BasePrice, Is.EqualTo(room.BasePrice));
            Assert.That(info.AvailableServices, Is.EqualTo(room.AvailableServices));
        });
    }
}