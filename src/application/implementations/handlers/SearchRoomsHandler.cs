using ABP.Application.Dto.Commands.SearchRoomsHandler;
using ABP.Application.Dto.Infos;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Result;

namespace ABP.Application.Implementations.Handlers;

public class SearchRoomsHandler(IBookingRepository bookingRepository, IRoomRepository roomRepository) : ISearchRoomsHandler
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IRoomRepository _roomRepository = roomRepository;

    public async Task<Result<IReadOnlyList<RoomInfo>>> SearchRoomsAsync(SearchRoomsCommand command)
    {
        var from = command.Date.ToDateTime(command.StartTime);
        var to = command.Date.ToDateTime(command.EndTime);

        // Get all rooms that satisfy the capacity requirement.
        var suitableRooms = (await _roomRepository.GetAllAsync())
            .Where(r => r.Capacity >= command.MinimalCapacity)
            .ToList();

        // Get bookings overlapping the requested period.
        var bookings = await _bookingRepository.FindByDateTimeAsync(from, to);

        // IDs of rooms that are already booked.
        var bookedRoomIds = bookings
            .Select(b => b.Room.Id)
            .ToHashSet();

        // Keep only rooms that aren't booked.
        var spareRooms = suitableRooms
            .Where(r => !bookedRoomIds.Contains(r.Id))
            .Select(r => new RoomInfo(
                r.Id,
                r.Name,
                r.Capacity,
                r.BasePrice,
                r.AvailableServices))
            .ToList();

        return Result<IReadOnlyList<RoomInfo>>.Success(spareRooms);
    }
}