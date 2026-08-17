using ABP.Domain.Entities;
using ABP.Domain.Result;
using ABP.Application.Dto.Commands.ManageRoomsHandler;

namespace ABP.Application.Interfaces.Handlers;

/// <summary>
/// Business rules contract that exposes functionality for rooms managing (creating, updating, removing). 
/// </summary>
public interface IManageRoomsHandler {
    public Task<Result<string>> CreateAsync(CreateRoomCommand command);
    public Task<Result> UpdateAsync(UpdateRoomCommand command);
    public Task<Result> RemoveAsync(DeleteRoomCommand command);
}