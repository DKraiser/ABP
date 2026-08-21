using System.Security.Principal;
using ABP.Application.Dto.Errors;
using ABP.Application.Dto.Infos;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using ABP.Domain.Result;

namespace ABP.Application.Implementations.Handlers;

public class ReportHandler(IBookingRepository bookingRepository, IRoomRepository roomRepository) : IReportHandler
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IRoomRepository _roomRepository = roomRepository;

    public async Task<Result<IEnumerable<RoomRevenueInfo>>> GetRoomRevenuesAsync(DateOnly from, DateOnly to)
    {
        if (from >= to)
        {
            var problems = new Dictionary<string, string[]>
            {
                ["Period"] = ["Requested period length should be positive."]
            };
            return Result<IEnumerable<RoomRevenueInfo>>.Failure(new DomainRulesViolationError(problems));
        }

        List<RoomRevenueInfo> info = [];

        var bookingsForPeriod = await _bookingRepository.FindByDateTimeStrictlyInAsync
            (from.ToDateTime(new(0, 0)), to.ToDateTime(new(0, 0))) ?? [];
        var roomIds = bookingsForPeriod.Select(b => b.Room.Id).Distinct() ?? [];

        foreach (string id in roomIds)
        {
            var total = bookingsForPeriod.Where(b => b.Room.Id == id).Sum(b => b.Price);
            Dictionary<string, decimal> serviceRevenues = [];
            var usedServices = bookingsForPeriod
                                            .Where(b => b.Room.Id == id)
                                            .SelectMany(b => b.RequestedServices);
            var usedServicesIds = usedServices.Select(s => s.Id).Distinct();
            foreach (var sid in usedServicesIds)
            {
                var serviceLastOccuredName = usedServices.Where(s => s.Id == sid).Last().Name;
                var serviceRevenue = usedServices.Where(s => s.Id == sid).Sum(s => s.Price);
                serviceRevenues.Add(serviceLastOccuredName, serviceRevenue);
            }
            info.Add(
                new (
                    id, 
                    total, 
                    serviceRevenues
                        .AsReadOnly()
                        .Select(p => new KeyValuePair<string, decimal>(p.Key, p.Value))
                        .ToDictionary()
                )
            );
        }

        return Result<IEnumerable<RoomRevenueInfo>>.Success(info);
    }

    public async Task<Result<IEnumerable<RoomUtilizationInfo>>> GetRoomUtilizationsAsync(DateOnly from, DateOnly to)
    {
        if (from >= to)
        {
            var problems = new Dictionary<string, string[]>
            {
                ["Period"] = ["Requested period length should be positive."]
            };
            return Result<IEnumerable<RoomUtilizationInfo>>.Failure(new DomainRulesViolationError(problems));
        }

        List<RoomUtilizationInfo> info = [];

        var bookingsForPeriod = await _bookingRepository.FindByDateTimeStrictlyInAsync
            (from.ToDateTime(new(0, 0)), to.ToDateTime(new(0, 0))) ?? [];
        var roomIds = bookingsForPeriod.Select(b => b.Room.Id).Distinct() ?? [];

        foreach (string id in roomIds)
        {
            var bookedHours = bookingsForPeriod
                                            .Where(b => b.Room.Id == id)
                                            .Sum(b => (b.EndTime - b.StartTime).TotalHours);
            var averageBookedHoursPerDay = bookedHours / (new DateTime(to, default) - new DateTime(from, default)).Days;
            info.Add(new(id, bookedHours, averageBookedHoursPerDay));
        }

        return Result<IEnumerable<RoomUtilizationInfo>>.Success(info);
    }
}