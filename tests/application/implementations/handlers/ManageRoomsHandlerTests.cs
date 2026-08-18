using ABP.Application.Dto.Commands.ManageRoomsHandler;
using ABP.Application.Dto.Errors;
using ABP.Application.Implementations.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using Moq;

namespace ABP.Tests.Application.Implementations.Handlers;

[TestFixture]
public class ManageRoomsHandlerTests
{
    private Mock<IRoomRepository> _repository = null!;
    private ManageRoomsHandler _handler = null!;

    private Room _room = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IRoomRepository>();
        _handler = new ManageRoomsHandler(_repository.Object);

        _room = new Room("A", 10, 100, []);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Test]
    public async Task CreateAsync_ValidCommand_CreatesRoom()
    {
        _repository
            .Setup(r => r.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Room?)null);

        _repository
            .Setup(r => r.AddAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        var command = new CreateRoomCommand("A", 10, 100, []);

        var result = await _handler.CreateAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        });

        _repository.Verify(
            r => r.AddAsync(It.IsAny<Room>()),
            Times.Once);
    }

    [Test]
    public async Task CreateAsync_ValidCommand_ReturnsCreatedRoomId()
    {
        _repository
            .Setup(r => r.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Room?)null);

        Room? addedRoom = null;

        _repository
            .Setup(r => r.AddAsync(It.IsAny<Room>()))
            .Callback<Room>(room => addedRoom = room)
            .Returns(Task.CompletedTask);

        var command = new CreateRoomCommand("A", 10, 100, []);

        var result = await _handler.CreateAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value, Is.EqualTo(addedRoom!.Id));
        });
    }

    [Test]
    public async Task CreateAsync_InvalidDomainData_ReturnsDomainRulesViolation()
    {
        // Assuming empty name violates Room's domain rules.
        var command = new CreateRoomCommand(
            "",
            10,
            100,
            []);

        var result = await _handler.CreateAsync(command);
        
        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Error, Is.TypeOf<DomainRulesViolationError>());
        });

        _repository.Verify(
            r => r.FindByIdAsync(It.IsAny<string>()),
            Times.Never);

        _repository.Verify(
            r => r.AddAsync(It.IsAny<Room>()),
            Times.Never);
    }

    [Test]
    public async Task CreateAsync_RoomAlreadyExists_ReturnsConflict()
    {
        _repository
            .Setup(r => r.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(_room);

        var command = new CreateRoomCommand("A", 10, 100, []);

        var result = await _handler.CreateAsync(command);
        
        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Error, Is.TypeOf<ConflictError>());
        });

        _repository.Verify(
            r => r.FindByIdAsync(It.IsAny<string>()),
            Times.Once);

        _repository.Verify(
            r => r.AddAsync(It.IsAny<Room>()),
            Times.Never);
    }


    // ============================================================
    // FindAsync
    // ============================================================

    [Test]
    public async Task FindAsync_ExistingRoom_ReturnsRoomInfo()
    {
        _repository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        var command = new FindRoomCommand(_room.Id);

        var result = await _handler.FindAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value!.Id, Is.EqualTo(_room.Id));
            Assert.That(result.Value.Name, Is.EqualTo(_room.Name));
            Assert.That(result.Value.Capacity, Is.EqualTo(_room.Capacity));
            Assert.That(result.Value.BasePrice, Is.EqualTo(_room.BasePrice));
        });
    }

    [Test]
    public async Task FindAsync_NonExistingRoom_ReturnsNotFound()
    {
        _repository
            .Setup(r => r.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Room?)null);

        var command = new FindRoomCommand("does-not-exist");

        var result = await _handler.FindAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Error, Is.TypeOf<NotFoundError>());
        });
    }


    // ============================================================
    // ListAllRoomsAsync
    // ============================================================

    [Test]
    public async Task ListAllRoomsAsync_ReturnsAllRooms()
    {
        var room1 = new Room("A", 10, 100, []);
        var room2 = new Room("B", 20, 200, []);

        _repository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new[] { room1, room2 });

        var result = await _handler.ListAllRoomsAsync();

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(2));

            Assert.That(result.Value![0].Id, Is.EqualTo(room1.Id));
            Assert.That(result.Value[0].Name, Is.EqualTo(room1.Name));

            Assert.That(result.Value[1].Id, Is.EqualTo(room2.Id));
            Assert.That(result.Value[1].Name, Is.EqualTo(room2.Name));
        });
    }

    [Test]
    public async Task ListAllRoomsAsync_NoRooms_ReturnsEmptyList()
    {
        _repository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([]);

        var result = await _handler.ListAllRoomsAsync();

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }


    // ============================================================
    // UpdateAsync
    // ============================================================

    [Test]
    public async Task UpdateAsync_ExistingRoom_UpdatesRoom()
    {
        _repository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _repository
            .Setup(r => r.UpdateAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateRoomCommand(_room.Id, "Updated room", 20, 200, null, null, null);

        var result = await _handler.UpdateAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);

            Assert.That(_room.Name, Is.EqualTo("Updated room"));
            Assert.That(_room.Capacity, Is.EqualTo(20));
            Assert.That(_room.BasePrice, Is.EqualTo(200));
        });

        _repository.Verify(
            r => r.UpdateAsync(_room),
            Times.Once);
    }

    [Test]
    public async Task UpdateAsync_NonExistingRoom_ReturnsNotFound()
    {
        _repository
            .Setup(r => r.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Room?)null);

        var command = new UpdateRoomCommand("does-not-exist", "Updated", 20, 200, null, null, null);

        var result = await _handler.UpdateAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Error, Is.TypeOf<NotFoundError>());
        });

        _repository.Verify(
            r => r.UpdateAsync(It.IsAny<Room>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateAsync_InvalidDomainData_ReturnsDomainRulesViolation()
    {
        _repository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        // Assuming empty name violates Room's domain rules.
        var command = new UpdateRoomCommand(_room.Id, "", 20, 200, null, null, null);

        var result = await _handler.UpdateAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(
                result.Error,
                Is.TypeOf<DomainRulesViolationError>());

            _repository.Verify(
                r => r.UpdateAsync(It.IsAny<Room>()),
                Times.Never);
        });
        
    }

    [Test]
    public async Task UpdateAsync_WithNewService_AddsService()
    {
        _repository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _repository
            .Setup(r => r.UpdateAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        var service = new Service("projector", 20);

        var command = new UpdateRoomCommand(_room.Id, null, 0, 0, [service], null, null);

        var result = await _handler.UpdateAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(_room.AvailableServices, Does.Contain(service));
        });

        _repository.Verify(
            r => r.UpdateAsync(_room),
            Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WithRemovedService_RemovesService()
    {
        var service = new Service("projector", 20);
        _room.AddService(service);

        _repository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _repository
            .Setup(r => r.UpdateAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateRoomCommand(_room.Id, null, 0, 0, null, null, [service.Id]);

        var result = await _handler.UpdateAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(
                _room.AvailableServices.Any(s => s.Id == service.Id),
                Is.False);
        });

        _repository.Verify(
            r => r.UpdateAsync(_room),
            Times.Once);
    }

    [Test]
    public async Task UpdateAsync_UnspecifiedFields_RemainsUnchanged()
    {
        _repository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _repository
            .Setup(r => r.UpdateAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        var originalCapacity = _room.Capacity;
        var originalPrice = _room.BasePrice;

        var command = new UpdateRoomCommand(_room.Id, "New name", 0, 0, null, null, null);

        var result = await _handler.UpdateAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(_room.Name, Is.EqualTo("New name"));
            Assert.That(_room.Capacity, Is.EqualTo(originalCapacity));
            Assert.That(_room.BasePrice, Is.EqualTo(originalPrice));
        });
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Test]
    public async Task DeleteAsync_ExistingRoom_DeletesRoom()
    {
        _repository
            .Setup(r => r.FindByIdAsync(_room.Id))
            .ReturnsAsync(_room);

        _repository
            .Setup(r => r.RemoveAsync(_room.Id))
            .Returns(Task.CompletedTask);

        var command = new DeleteRoomCommand(_room.Id);

        var result = await _handler.DeleteAsync(command);

        Assert.That(result.IsSuccessful, Is.True);

        _repository.Verify(
            r => r.RemoveAsync(_room.Id),
            Times.Once);
    }

    [Test]
    public async Task DeleteAsync_NonExistingRoom_ReturnsNotFound()
    {
        _repository
            .Setup(r => r.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Room?)null);

        var command = new DeleteRoomCommand("does-not-exist");

        var result = await _handler.DeleteAsync(command);

        Assert.Multiple(() => {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Error, Is.TypeOf<NotFoundError>());
        });

        _repository.Verify(
            r => r.RemoveAsync(It.IsAny<string>()),
            Times.Never);
    }
}