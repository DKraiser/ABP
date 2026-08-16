using ABP.Application.Dto.Commands.ManageRoomsHandler;
using ABP.Application.Dto.Errors.ManageRooms;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using ABP.Domain.Result;

namespace ABP.Application.Implementations.Handlers;

public class ManageRoomsHandler (IRoomRepository repository) : IManageRoomsHandler
{
    private IRoomRepository _repository = repository; 

    public async Task<Result<string>> AddAsync(CreateRoomCommand command)
    {
        Dictionary<string, string[]>? validationProblems = [];

        if (string.IsNullOrWhiteSpace(command.Name)) 
            validationProblems.Add("Name", ["Room name cannot be empty."]);
        if (command.Capacity == 0)
            validationProblems.Add("Capacity", ["Room capacity must be greater than zero."]);
        if (command.BasePrice == 0)
            validationProblems.Add("Price", ["Room base price must be greater than zero."]);
        
        if (validationProblems.Count is not 0)
            return Result<string>.Failure(new AddRoomError(validationProblems));
        
        Room room = new (command.Name, command.Capacity, command.BasePrice, command.Services);
        await _repository.AddAsync(room);

        return Result<string>.Success(room.Id);
    }

    public Task<Result> DeleteAsync(DeleteRoomCommand command)
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdateAsync(UpdateRoomCommand command)
    {
        throw new NotImplementedException();
    }
}