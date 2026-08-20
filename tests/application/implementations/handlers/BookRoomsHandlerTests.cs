using ABP.Application.Dto.Commands.BookRoomsHandler;
using ABP.Application.Dto.Errors;
using ABP.Application.Implementations.Handlers;
using ABP.Application.Interfaces.Policies;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using Moq;

[TestFixture]
public class BookRoomsHandlerTests
{
    private Mock<IBookingRepository> _bookingRepository = null!;
    private Mock<IRoomRepository> _roomRepository = null!;

    private Mock<IBookingPolicy> _bookingPolicy = null!;
    private Mock<IPricingPolicy> _pricingPolicy = null!;

    private BookRoomsHandler _handler = null!;

    private Room _room = null!;
    private Service _service = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = new Mock<IBookingRepository>();
        _roomRepository = new Mock<IRoomRepository>();

        _bookingPolicy = new Mock<IBookingPolicy>();
        _pricingPolicy = new Mock<IPricingPolicy>();

        _room = new Room("A", 10, 100, []);

        _service = new Service("Projector", 20);
        _room.AddService(_service);

        _handler = new BookRoomsHandler(
            _bookingRepository.Object,
            _roomRepository.Object,
            [_bookingPolicy.Object],
            [_pricingPolicy.Object]);
    }

    private BookRoomCommand CreateCommand(
        string roomId = "",
        DateOnly? date = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        List<string>? serviceIds = null)
    {
        return new BookRoomCommand(
            roomId == "" ? _room.Id : roomId,
            date ?? new DateOnly(2026, 8, 20),
            startTime ?? new TimeOnly(10, 0),
            endTime ?? new TimeOnly(12, 0),
            serviceIds ?? []);
    }


    // ============================================================
    // Room lookup
    // ============================================================

    [Test]
    public async Task BookRoomAsync_RoomDoesNotExist_ReturnsNotFound()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Room?)null);

        var command = CreateCommand("does-not-exist");

        var result = await _handler.BookRoomAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Error, Is.TypeOf<NotFoundError>());
        });

        _bookingRepository.Verify(
            r => r.AddAsync(It.IsAny<Booking>()),
            Times.Never);
    }


    // ============================================================
    // Services
    // ============================================================

    [Test]
    public async Task BookRoomAsync_RequestedServiceDoesNotExist_ReturnsNotFound()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        var command = CreateCommand(
            serviceIds: ["does-not-exist"]);

        var result = await _handler.BookRoomAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Error, Is.TypeOf<NotFoundError>());
        });

        _bookingRepository.Verify(
            r => r.AddAsync(It.IsAny<Booking>()),
            Times.Never);
    }

    [Test]
    public async Task BookRoomAsync_OneValidAndOneInvalidService_ReturnsNotFound()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        var command = CreateCommand(
            serviceIds: [_service.Id, "does-not-exist"]);

        var result = await _handler.BookRoomAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Error, Is.TypeOf<NotFoundError>());
        });

        _bookingRepository.Verify(
            r => r.AddAsync(It.IsAny<Booking>()),
            Times.Never);
    }


    // ============================================================
    // Domain rules
    // ============================================================

    [Test]
    public async Task BookRoomAsync_InvalidBooking_ReturnsDomainRulesViolation()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        // Adjust these values according to your actual Booking
        // domain rules. For example, if end <= start is invalid:
        var command = CreateCommand(
            startTime: new TimeOnly(12, 0),
            endTime: new TimeOnly(10, 0));

        var result = await _handler.BookRoomAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(
                result.Error,
                Is.TypeOf<DomainRulesViolationError>());
        });

        _bookingRepository.Verify(
            r => r.AddAsync(It.IsAny<Booking>()),
            Times.Never);
    }


    // ============================================================
    // Booking policies
    // ============================================================

    [Test]
    public async Task BookRoomAsync_BookingPolicyRejectsBooking_ReturnsBusinessRulesViolation()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _bookingPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(false);

        var command = CreateCommand();

        var result = await _handler.BookRoomAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(
                result.Error,
                Is.TypeOf<BusinessRulesViolationError>());
        });

        _bookingRepository.Verify(
            r => r.AddAsync(It.IsAny<Booking>()),
            Times.Never);

        _pricingPolicy.Verify(
            p => p.CalculatePrice(It.IsAny<Booking>()),
            Times.Never);
    }

    [Test]
    public async Task BookRoomAsync_AllBookingPoliciesAllowBooking_Continues()
    {
        var secondPolicy = new Mock<IBookingPolicy>();

        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _bookingPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(true);

        secondPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(true);

        _bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([]);

        _pricingPolicy
            .Setup(p => p.CalculatePrice(It.IsAny<Booking>()))
            .Returns(100);

        var handler = new BookRoomsHandler(
            _bookingRepository.Object,
            _roomRepository.Object,
            [_bookingPolicy.Object, secondPolicy.Object],
            [_pricingPolicy.Object]);

        var result = await handler.BookRoomAsync(CreateCommand());

        Assert.That(result.IsSuccessful, Is.True);

        _bookingPolicy.Verify(
            p => p.IsAllowed(It.IsAny<Booking>()),
            Times.Once);

        secondPolicy.Verify(
            p => p.IsAllowed(It.IsAny<Booking>()),
            Times.Once);
    }


    // ============================================================
    // Booking conflicts
    // ============================================================

    [Test]
    public async Task BookRoomAsync_OverlappingBookingExists_ReturnsConflict()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _bookingPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(true);

        var existingBooking = new Booking(
            _room,
            new DateTime(2026, 8, 20, 11, 0, 0),
            new DateTime(2026, 8, 20, 13, 0, 0),
            []);

        _bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([existingBooking]);

        var command = CreateCommand(
            startTime: new TimeOnly(10, 0),
            endTime: new TimeOnly(12, 0));

        var result = await _handler.BookRoomAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Error, Is.TypeOf<ConflictError>());
        });

        _bookingRepository.Verify(
            r => r.AddAsync(It.IsAny<Booking>()),
            Times.Never);
    }

    [Test]
    public async Task BookRoomAsync_NoOverlappingBooking_Continues()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _bookingPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(true);

        _bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([]);

        _pricingPolicy
            .Setup(p => p.CalculatePrice(It.IsAny<Booking>()))
            .Returns(100);

        var result = await _handler.BookRoomAsync(CreateCommand());

        Assert.That(result.IsSuccessful, Is.True);
    }


    // ============================================================
    // Pricing
    // ============================================================

    [Test]
    public async Task BookRoomAsync_CalculatesPrice()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _bookingPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(true);

        _bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([]);

        _pricingPolicy
            .Setup(p => p.CalculatePrice(It.IsAny<Booking>()))
            .Returns(150);

        var result = await _handler.BookRoomAsync(CreateCommand());

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value!.Price, Is.EqualTo(150));
        });

        _pricingPolicy.Verify(
            p => p.CalculatePrice(It.IsAny<Booking>()),
            Times.Once);
    }

    [Test]
    public async Task BookRoomAsync_MultiplePricingPolicies_SumsPrices()
    {
        var secondPolicy = new Mock<IPricingPolicy>();

        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _bookingPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(true);

        _bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([]);

        _pricingPolicy
            .Setup(p => p.CalculatePrice(It.IsAny<Booking>()))
            .Returns(100);

        secondPolicy
            .Setup(p => p.CalculatePrice(It.IsAny<Booking>()))
            .Returns(50);

        var handler = new BookRoomsHandler(
            _bookingRepository.Object,
            _roomRepository.Object,
            [_bookingPolicy.Object],
            [_pricingPolicy.Object, secondPolicy.Object]);

        var result = await handler.BookRoomAsync(CreateCommand());

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value!.Price, Is.EqualTo(150));
        });
    }


    // ============================================================
    // Successful booking
    // ============================================================

    [Test]
    public async Task BookRoomAsync_ValidBooking_StoresBooking()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _bookingPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(true);

        _bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([]);

        _pricingPolicy
            .Setup(p => p.CalculatePrice(It.IsAny<Booking>()))
            .Returns(100);

        var command = CreateCommand();

        var result = await _handler.BookRoomAsync(command);

        Assert.That(result.IsSuccessful, Is.True);

        _bookingRepository.Verify(
            r => r.AddAsync(It.IsAny<Booking>()),
            Times.Once);
    }

    [Test]
    public async Task BookRoomAsync_ValidBooking_ReturnsConfirmation()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _bookingPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(true);

        _bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([]);

        _pricingPolicy
            .Setup(p => p.CalculatePrice(It.IsAny<Booking>()))
            .Returns(120);

        var command = CreateCommand(
            date: new DateOnly(2026, 8, 20),
            startTime: new TimeOnly(14, 0),
            endTime: new TimeOnly(16, 30));

        var result = await _handler.BookRoomAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);

            Assert.That(result.Value!.Price, Is.EqualTo(120));
            Assert.That(result.Value.Date, Is.EqualTo(command.Date));
            Assert.That(result.Value.StartTime, Is.EqualTo(command.StartTime));
            Assert.That(result.Value.EndTime, Is.EqualTo(command.EndTime));
            Assert.That(result.Value.Id, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task BookRoomAsync_WithRequestedServices_AddsServicesToBooking()
    {
        _roomRepository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _bookingPolicy
            .Setup(p => p.IsAllowed(It.IsAny<Booking>()))
            .Returns(true);

        _bookingRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([]);

        _pricingPolicy
            .Setup(p => p.CalculatePrice(It.IsAny<Booking>()))
            .Returns(100);

        Booking? createdBooking = null;

        _bookingRepository
            .Setup(r => r.AddAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b => createdBooking = b)
            .Returns(Task.CompletedTask);

        var command = CreateCommand(
            serviceIds: [_service.Id]);

        var result = await _handler.BookRoomAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(createdBooking, Is.Not.Null);
            Assert.That(createdBooking!.RequestedServices, Has.Count.EqualTo(1));
            Assert.That(
                createdBooking.RequestedServices[0].Id,
                Is.EqualTo(_service.Id));
        });
    }
}