using ABP.Domain.Entities;
using ABP.Domain.Result;
using ABP.Application.Dto.Commands.ManageRoomsHandler;

namespace ABP.Application.Interfaces.Handlers;

/// <summary>
/// CRUD handler for rooms. 
/// </summary>
public interface IManageRoomsHandler {
    public Task<Result<string>> Add(CreateRoomCommand command);
    public Task<Result> Update(UpdateRoomCommand command);
    public Task<Result> Delete(DeleteRoomCommand command);
}