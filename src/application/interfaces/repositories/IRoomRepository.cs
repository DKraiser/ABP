using ABP.Domain.Entities;
using ABP.Domain.Result;

namespace ABP.Application.Interfaces.Repositories;

/// <summary>
/// Abstraction of storage of rooms. 
/// </summary>
public interface IRoomRepository { 
    public Task AddAsync(Room room);
    public Task UpdateAsync(Room room);
    public Task RemoveAsync(string id);
    public Task<Room?> FindByIdAsync(string id);
    public Task<IEnumerable<Room>> GetAllAsync();
}