using ABP.Domain.Entities;
using ABP.Domain.Result;

namespace ABP.Application.Interfaces.Repositories;

/// <summary>
/// Abstraction of storage of booking records. 
/// </summary>
public interface IBookingRepository { 
    public Task Add(Booking room);
    public Task Update(Booking room);
    public Task Remove(string id);
    public Task<Booking?> FindById(string id);
    public Task<IEnumerable<Booking>> FindByDateTime(DateTime from, DateTime to);
}