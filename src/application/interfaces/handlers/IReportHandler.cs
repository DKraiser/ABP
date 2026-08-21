using ABP.Application.Dto.Infos;
using ABP.Domain.Result;

namespace ABP.Application.Interfaces.Handlers;

/// <summary>
/// Some useful reports for business.
/// </summary>
public interface IReportHandler
{
    /// <summary>
    /// Lists how many hours of booking in stated period each room has had.
    /// </summary>
    /// <param name="from">Start of period.</param>
    /// <param name="to">End of period.</param>
    public Task<Result<IEnumerable<RoomUtilizationInfo>>> GetRoomUtilizationsAsync(DateOnly from, DateOnly to);

    /// <summary>
    /// Lists total revenue in stated period each room has had.
    /// </summary>
    /// <param name="from">Start of period.</param>
    /// <param name="to">End of period.</param>
    public Task<Result<IEnumerable<RoomRevenueInfo>>> GetRoomRevenuesAsync(DateOnly from, DateOnly to);
}