using ABP.Domain.Entities;
using ABP.Domain.Result;

namespace ABP.Application.Interfaces.Repositories;

/// <summary>
/// Abstraction of storage of booking records. 
/// </summary>
public interface IBookingRepository
{
    public Task AddAsync(Booking room);
    public Task UpdateAsync(Booking room);
    public Task DeleteAsync(string id);
    public Task<Booking?> FindByIdAsync(string id);
    public Task<IReadOnlyCollection<Booking>> FindByDateTimeStrictlyInAsync(DateTime from, DateTime to);
    public Task<IReadOnlyCollection<Booking>> FindByDateTimeOverlappingAsync(DateTime from, DateTime to);
    public Task<IReadOnlyCollection<Booking>> GetAllAsync();
}