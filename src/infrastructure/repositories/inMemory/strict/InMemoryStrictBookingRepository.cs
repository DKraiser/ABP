using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;

namespace ABP.Infrastructure.Repositories.InMemory.Strict;

public class InMemoryStrictBookingRepository : IBookingRepository {
    private readonly List<Booking> _repository = [];

    /// <summary>
    /// Adds a new booking to repository.
    /// </summary>
    /// <remarks>
    /// Duplicate of the booking is stored. 
    /// </remarks>
    /// <param name="booking">Booking to be stored.</param>
    /// <exception cref="InvalidArgumentException">if the booking with this id already exists.</exception>
    public async Task AddAsync(Booking booking)
    {
        if (_repository.Find(b => b.Id == booking.Id) is null)
            _repository.Add(booking);
        else throw new InvalidOperationException("Booking with this id already exists.");
    }

    /// <summary>
    /// Removes booking with `id` to repository.
    /// </summary>
    /// <param name="booking">Id of booking to be removed.</param>
    /// <exception cref="InvalidArgumentException">if the booking with this id does not exist.</exception>
    public async Task RemoveAsync(string id)
    {
        if (_repository.RemoveAll(b => b.Id == id) is 0) 
            throw new InvalidOperationException("Booking with this id does not exist.");       
    }

    /// <summary>
    /// Finds booking in repository by id.
    /// </summary>
    /// <remarks>
    /// Duplicate of the booking is returned. 
    /// </remarks>
    /// <param name="id">Id of booking to find.</param>
    public async Task<Booking?> FindByIdAsync(string id)
    {
        var booking = _repository.Find(b => b.Id == id);
        
        if (booking is null) return null;
        else return new (booking);
    }

    /// <summary>
    /// Finds booking in repository by id.
    /// </summary>
    /// <remarks>
    /// Duplicate of the booking is returned. 
    /// </remarks>
    /// <param name="from">Lower limit of search.</param>
    /// <param name="to">Upper limit of search.</param>
    public async Task<IEnumerable<Booking>> FindByDateTimeAsync(DateTime from, DateTime to) {
        return _repository.FindAll(b => b.StartTime >= from && b.EndTime <= to);
    }

    /// <summary>
    /// Updates booking data.
    /// </summary>
    /// <param name="booking">Updated booking data.</param>
    /// <exception cref="InvalidOperationException">if booking with this id does not exist.</exception>
    public async Task UpdateAsync(Booking booking)
    {
        if (_repository.Find(b => b.Id == booking.Id) is null)
            throw new InvalidOperationException("Booking with this id does not exist.");
        
        _repository[_repository.FindIndex(b => b.Id == booking.Id)] = booking;
    }

    /// <summary>
    /// Get all repository elements.
    /// </summary>
    /// <returns>Collection of repository elements.</returns>
    public async Task<IEnumerable<Booking>> GetAllAsync() { 
        if (_repository.Count is 0) return [];
        var result = new List<Booking>();
        result.AddRange(_repository.Select(b => new Booking(b)));
        return result;
    }
}