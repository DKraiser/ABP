using ABP.Application.Dto.Infos;
using ABP.Application.Dto.Commands.SearchRoomsHandler;
using ABP.Domain.Entities;
using ABP.Domain.Result;

namespace ABP.Application.Interfaces.Handlers;

/// <summary>
/// Search rooms handler. 
/// </summary>
public interface ISearchAvailableRoomsHandler
{
    public Task<Result<IReadOnlyList<RoomInfo>>> SearchAvailableAsync(SearchAvailableRoomsCommand command);
}