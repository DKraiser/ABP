using ABP.Domain.Result;
using ABP.Application.Dto.Commands.ManageRoomsHandler;
using ABP.Application.Dto.Infos;

namespace ABP.Application.Interfaces.Handlers;

/// <summary>
/// Business rules contract that exposes CRUD functionality for rooms managing. 
/// </summary>
public interface IManageRoomsHandler {
    public Task<Result<string>> CreateAsync(CreateRoomCommand command);
    public Task<Result<IReadOnlyList<RoomInfo>>> ListAllRoomsAsync();
    public Task<Result<RoomInfo>> FindAsync(FindRoomCommand command);
    public Task<Result> UpdateAsync(UpdateRoomCommand command);
    public Task<Result> DeleteAsync(DeleteRoomCommand command);
}