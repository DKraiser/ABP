using ABP.Application.Dto.Infos;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Repositories;

namespace ABP.Application.Implementations.Handlers;

public class ReportHandler(IBookingRepository bookingRepository, IRoomRepository roomRepository) : IReportHandler
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IRoomRepository _roomRepository = roomRepository;
    public async Task<IReadOnlyList<RoomUtilizationInfo>> GetRoomUtilizationsAsync(DateOnly from, DateOnly to)
    {
        List<RoomUtilizationInfo> info = [];

        var bookingsForPeriod = await _bookingRepository.FindByDateTimeStrictlyInAsync(from.ToDateTime(new(0, 0)), to.ToDateTime(new(0, 0)));
        var roomIds = (await _roomRepository.GetAllAsync()).Select(r => r.Id);

        foreach (string id in roomIds)
            info.Add(new(id, bookingsForPeriod.Where(b => b.Room.Id == id).Sum(b => (b.EndTime - b.StartTime).TotalHours)));

        return info;
    }
}