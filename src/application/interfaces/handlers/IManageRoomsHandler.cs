using ABP.Domain.Entities;
using ABP.Domain.Result;
using ABP.Application.Dto.Commands.ManageRoomsHandler;

namespace ABP.Application.Interfaces.Handlers;

/// <summary>
/// CRUD handler for rooms. 
/// </summary>
public interface IManageRoomsHandler {
    public Task<Result<string>> AddAsync(CreateRoomCommand command);
    public Task<Result> UpdateAsync(UpdateRoomCommand command);
    public Task<Result> DeleteAsync(DeleteRoomCommand command);
}