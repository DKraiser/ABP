using ABP.Application.Dto.Commands.SearchRoomsHandler;
using ABP.Application.Dto.Errors;
using ABP.Application.Dto.Infos;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Result;

namespace ABP.Application.Implementations.Handlers;

public class SearchRoomsHandler(IBookingRepository bookingRepository, IRoomRepository roomRepository) : ISearchRoomsHandler
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IRoomRepository _roomRepository = roomRepository;

    /// <summary>
    /// Searches the spare room.
    /// </summary>
    /// <param name="command">Criteria room have to match.</param>
    /// <returns>`Result<string>.Success()` with `id` if created successfully.</returns>
    /// <returns>`Result<string>.Failure(DomainRulesViolationError)` if criteria violate domain rules.</returns>
    public async Task<Result<IReadOnlyList<RoomInfo>>> SearchRoomsAsync(SearchRoomsCommand command)
    {
        var from = command.Date.ToDateTime(command.StartTime);
        var to = command.Date.ToDateTime(command.EndTime);

        if (from >= to)
        {
            return Result<IReadOnlyList<RoomInfo>>.Failure(
                new DomainRulesViolationError(
                    new Dictionary<string, string[]>()
                    {
                        ["Booking period"] = ["Booking period length must be positive"]
                    }
                )
            );
        }

        if (command.MinimalCapacity < 0)
        {
            return Result<IReadOnlyList<RoomInfo>>.Failure(
                new DomainRulesViolationError(
                    new Dictionary<string, string[]>()
                    {
                        ["Minimal capacity"] = ["Minimal capacity must be greater than zero."]
                    }
                )
            );
        }

        // Get all rooms that satisfy the capacity requirement.
        var suitableRooms = (await _roomRepository.GetAllAsync())
            .Where(r => r.Capacity >= command.MinimalCapacity)
            .ToList();

        // Get bookings overlapping the requested period.
        var bookings = await _bookingRepository.FindByDateTimeOverlappingAsync(from, to);

        // IDs of rooms that are already booked.
        var bookedRoomIds = bookings?
            .Select(b => b.Room.Id)
            .ToHashSet() ?? [];

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