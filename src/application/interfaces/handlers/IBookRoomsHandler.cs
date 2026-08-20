using ABP.Application.Dto.Infos;
using ABP.Application.Dto.Commands.BookRoomsHandler;
using ABP.Domain.Entities;
using ABP.Domain.Result;

namespace ABP.Application.Interfaces.Handlers;

/// <summary>
/// Searching and booking rooms handler. 
/// </summary>
public interface IBookRoomsHandler
{
    public Task<Result<BookingConfirmationInfo>> BookRoomAsync(BookRoomCommand command);
}