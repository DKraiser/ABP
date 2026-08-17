using ABP.Application.Dto.Commands.ManageRoomsHandler;
using ABP.Application.Dto.Errors;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using ABP.Domain.Result;
using ABP.Domain.Exceptions;
using ABP.Application.Exceptions;
using System.Runtime.InteropServices;

namespace ABP.Application.Implementations.Handlers;

public class ManageRoomsHandler (IRoomRepository repository) : IManageRoomsHandler
{
    private readonly IRoomRepository _repository = repository; 

    public async Task<Result<string>> CreateAsync(CreateRoomCommand command)
    {       
        Room room;
        try { 
            // Room is a domain object, so if one of domain rules is violated, 
            // `DomainRulesViolationException` is thrown.
            room = new (command.Name, command.Capacity, command.BasePrice, command.Services ?? []);
        } 
        catch (DomainRulesViolationException exception) {
            Dictionary<string, string[]>? domainProblems = [];
            domainProblems.Add("Failed to create the room.", [$"{exception.Message}"]);
            
            return Result<string>.Failure(new DomainRulesViolationError(domainProblems));
        }

        try {
            // If repository fails to add due to duplication etc,
            // it is an infrastructure layer problem and throws `RepositoryException`.
            await _repository.AddAsync(room);
        } 
        catch (RepositoryException exception) {
            Dictionary<string, string[]>? infrastructureProblems = [];
            infrastructureProblems.Add("Failed to store the room.", [$"{exception.Message}"]);
            
            return Result<string>.Failure(new InfrastructureError(infrastructureProblems));
        }

        // If all is ok, room id is returned.
        return Result<string>.Success(room.Id);
    }

    public async Task<Result> RemoveAsync(DeleteRoomCommand command)
    {
        try {
            // If repository fails to remove the room because of some problems (invalid id),
            // it is an infrastructure layer problem and throws `RepositoryException`.
            await _repository.RemoveAsync(command.Id);
        } 
        catch (RepositoryException exception) {
            Dictionary<string, string[]>? infrastructureProblems = [];
            infrastructureProblems.Add("Failed to remove the room.", [$"{exception.Message}"]);
            
            return Result.Failure(new InfrastructureError(infrastructureProblems));
        }

        // If all is ok, success is returned.
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(UpdateRoomCommand command)
    {
        // If room to update does not exist, return failure
        Room? updated = await _repository.FindByIdAsync(command.Id);
        if (updated is null) 
            return Result.Failure(new NotFoundError());

        // If some fields are on their default values - this setting should not be changed.
        try {
            if (command.NewName is not null)
                updated.Name = command.NewName;
            if (command.NewCapacity != 0)
                updated.Capacity = command.NewCapacity;
            if (command.NewBasePrice != 0)
                updated.BasePrice = command.NewBasePrice;

            command.NewServices?.ForEach(s => updated.AddService(s));
            command.RemovedServices?.ForEach(s => updated.RemoveService(s));
            command.UpdatedServices?.ForEach(s => updated.UpdateService(s));
        }
        catch (DomainRulesViolationException exception) {
            Dictionary<string, string[]>? domainProblems = [];
            domainProblems.Add("Failed to create the room.", [$"{exception.Message}"]);
            
            return Result<string>.Failure(new DomainRulesViolationError(domainProblems));
        }
        
        try {
            // If repository fails to update the room because of some problems (invalid id),
            // it is an infrastructure layer problem and throws `RepositoryException`.
            await _repository.UpdateAsync(updated);
        } 
        catch (RepositoryException exception) {
            Dictionary<string, string[]>? infrastructureProblems = [];
            infrastructureProblems.Add("Failed to remove the room.", [$"{exception.Message}"]);
            
            return Result.Failure(new InfrastructureError(infrastructureProblems));
        }

        // If all is ok, success is returned.
        return Result.Success();
    }
}