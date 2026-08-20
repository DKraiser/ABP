using ABP.Application.Dto.Commands.ManageRoomsHandler;
using ABP.Application.Dto.Errors;
using ABP.Application.Dto.Infos;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using ABP.Domain.Exceptions;
using ABP.Domain.Result;

namespace ABP.Application.Implementations.Handlers;

public class ManageRoomsHandler(IRoomRepository repository) : IManageRoomsHandler
{
    private readonly IRoomRepository _repository = repository;

    /// <summary>
    /// Create and save a room.
    /// </summary>
    /// <param name="command">Data of room to create.</param>
    /// <returns>`Result<string>.Success()` with `id` if created successfully.</returns>
    /// <returns>`Result<string>.Failure(DomainRulesViolationError)` if room data violate domain rules.</returns>
    /// <returns>`Result<string>.Failure(ConflictError)` if room already exists.</returns>
    public async Task<Result<string>> CreateAsync(CreateRoomCommand command)
    {
        Room room;
        try
        {
            // Room is a domain object, so if one of domain rules is violated, 
            // `DomainRulesViolationException` is thrown.
            room = new(command.Name, command.Capacity, command.BasePrice, command.Services ?? []);
        }
        catch (DomainRulesViolationException exception)
        {
            var domainProblems = new Dictionary<string, string[]>
            {
                ["Failed to create the room."] = [$"{exception.Message}"]
            };

            return Result<string>.Failure(new DomainRulesViolationError(domainProblems));
        }

        // If room with this id already exists, request create a conflict,
        // and the corresponding failure is returned.
        if (await _repository.FindByIdAsync(room.Id) is not null)
        {
            var duplicateProblems = new Dictionary<string, string[]>
            {
                ["Room"] = ["Room with this id already exists."]
            };

            return Result<string>.Failure(new ConflictError(duplicateProblems));
        }

        // If repository fails to add due to duplication etc,
        // it is an infrastructure layer problem and throws `RepositoryException`.
        // If `RepositoryException` was thrown, method should be debugged.  
        await _repository.AddAsync(room);

        // If all is ok, room id is returned.
        return Result<string>.Success(room.Id);
    }

    /// <summary>
    /// Finds a room with the specified id.
    /// </summary>
    /// <param name="command">Command with the id.</param>
    /// <returns>`Result<string>.Success()` with `RoomInfo` if found.</returns>
    /// <returns>`Result<string>.Failure(NotFoundError)` if room with this id does not exist.</returns>
    public async Task<Result<RoomInfo>> FindAsync(FindRoomCommand command)
    {
        Room? foundRoom = await _repository.FindByIdAsync(command.Id);
        if (foundRoom is null)
        {
            // If room was not found, return a failure.
            var notFoundProblems = new Dictionary<string, string[]>
            {
                ["Room"] = ["Room with this id does not exist."]
            };

            return Result<RoomInfo>.Failure(new NotFoundError(notFoundProblems));
        }

        return Result<RoomInfo>.Success(
            new(foundRoom.Id, foundRoom.Name, foundRoom.Capacity, foundRoom.BasePrice, foundRoom.AvailableServices));
    }

    /// <summary>
    /// Returns a collection of all rooms info.
    /// </summary>
    /// <returns>`Result<string>.Success()` with `IReadOnlyList<RoomInfo>`.</returns>
    public async Task<Result<IReadOnlyList<RoomInfo>>> ListAllRoomsAsync()
    {
        List<RoomInfo> roomInfos = [];
        var rooms = await _repository.GetAllAsync();
        rooms.ToList().ForEach(r => roomInfos.Add(
            new(r.Id, r.Name, r.Capacity, r.BasePrice, r.AvailableServices)
        ));
        return Result<IReadOnlyList<RoomInfo>>.Success(roomInfos);
    }

    /// <summary>
    /// Updates the room with the specified id.
    /// </summary>
    /// <param name="command">Command with the new data of room.</param>
    /// <returns>`Result<string>.Success()` if succeeded.</returns>
    /// <returns>`Result<string>.Failure(NotFoundError)` if room with this id does not exist.</returns>
    /// <returns>`Result<string>.Failure(DomainRulesViolationError)` if room data violate domain rules.</returns>
    public async Task<Result> UpdateAsync(UpdateRoomCommand command)
    {
        // If room to update does not exist, return failure
        Room? updated = await _repository.FindByIdAsync(command.Id);
        if (updated is null)
        {
            // If room was not found, return a failure.
            var notFoundProblems = new Dictionary<string, string[]>
            {
                ["Room"] = ["Room with this id does not exist."]
            };

            return Result.Failure(new NotFoundError(notFoundProblems));
        }

        // If some fields are on their default values - this setting should not be changed.
        try
        {
            if (command.NewName is not null)
                updated.Name = command.NewName;
            if (command.NewCapacity != 0)
                updated.Capacity = command.NewCapacity;
            if (command.NewBasePrice != 0)
                updated.BasePrice = command.NewBasePrice;

            foreach (var s in command.NewServices ?? [])
                updated.AddService(s);
            foreach (var s in command.UpdatedServices ?? [])
                updated.UpdateService(s);
            foreach (var id in command.RemovedServices ?? [])
                updated.RemoveService(id);
        }
        catch (DomainRulesViolationException exception)
        {
            var domainProblems = new Dictionary<string, string[]>()
            {
                ["Failed to update the room."] = [$"{exception.Message}"]
            };

            return Result<string>.Failure(new DomainRulesViolationError(domainProblems));
        }

        // If repository fails to update the room because of some problems (invalid id),
        // it is an infrastructure layer problem and throws `RepositoryException`.
        await _repository.UpdateAsync(updated);

        // If all is ok, success is returned.
        return Result.Success();
    }

    /// <summary>
    /// Updates the room with the specified id.
    /// </summary>
    /// <param name="command">Command with the new data of room.</param>
    /// <returns>`Result<string>.Success()` if succeeded.</returns>
    /// <returns>`Result<string>.Failure(NotFoundError)` if room with this id does not exist.</returns>
    public async Task<Result> DeleteAsync(DeleteRoomCommand command)
    {
        // If room to remove does not exist, return failure
        Room? removed = await _repository.FindByIdAsync(command.Id);
        if (removed is null)
        {
            // If room was not found, return a failure.
            var notFoundProblems = new Dictionary<string, string[]>
            {
                ["Room"] = ["Room with this id does not exist."]
            };

            return Result.Failure(new NotFoundError(notFoundProblems));
        }

        // If repository fails to remove the room because of some problems (invalid id),
        // it is an infrastructure layer problem and throws `RepositoryException`.
        await _repository.RemoveAsync(command.Id);

        // If all is ok, success is returned.
        return Result.Success();
    }
}