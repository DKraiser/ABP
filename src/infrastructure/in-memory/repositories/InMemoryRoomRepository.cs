using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;

namespace ABP.Infrastructure.InMemory.Repositories;

/// <summary>
/// In-memory room storage implementation.
/// </summary>
public class InMemoryRoomRepository : IRoomRepository {
    private readonly List<Room> _repository = [];

    /// <summary>
    /// Adds a new room to repository.
    /// </summary>
    /// <remarks>
    /// Duplicate of the room is stored. If the room with this id already exists, operation is ignored. 
    /// </remarks>
    /// <param name="room">Room to be stored.</param>
    public async Task AddAsync(Room room)
    {
        if (_repository.Find(r => r.Id == room.Id) is null)
            _repository.Add(new (room));
    }

    /// <summary>
    /// Removes room with `id` to repository.
    /// </summary>
    /// <remarks>
    /// If the room with this id does not exist, operation is ignored. 
    /// </remarks>
    /// <param name="room">Id of room to be removed.</param>
    public async Task RemoveAsync(string id)
    {
        _repository.RemoveAll(r => r.Id == id);
    }

    /// <summary>
    /// Finds room in repository by id.
    /// </summary>
    /// <remarks>
    /// Duplicate of the room is returned. 
    /// </remarks>
    /// <param name="id">Id of room to find.</param>
    public async Task<Room?> FindByIdAsync(string id)
    {
        var room = _repository.Find(r => r.Id == id);

        if (room is null) return null;
        else return new (room);
    }

    /// <summary>
    /// Updates room data.
    /// </summary>
    /// <exception cref="ArgumentException">if room with this id does not exist.</exception>
    /// <param name="room">Updated room data.</param>
    public async Task UpdateAsync(Room room)
    {
        if (_repository.Find(r => r.Id == room.Id) is null)
            throw new InvalidOperationException("Room with this id does not exist.");
        
        _repository[_repository.FindIndex(r => r.Id == room.Id)] = room;
    }

    /// <summary>
    /// Get all repository elements.
    /// </summary>
    /// <returns>Collection of repository elements.</returns>
    public async Task<IEnumerable<Room>> GetAllAsync() { 
        if (_repository.Count is 0) return [];
        var result = new List<Room>();
        result.AddRange(_repository.Select(r => new Room(r)));
        return result;
    }
}