using ABP.Domain.Entities;
using ABP.Domain.Result;

namespace ABP.Application.Interfaces.Repositories;

/// <summary>
/// Abstraction of storage of rooms. 
/// </summary>
public interface IRoomRepository { 
    public Task<string> Add(Room room);
    public Task Update(Room room);
    public Task Delete(string id);
    public Task<Room?> FindById(string id);
}