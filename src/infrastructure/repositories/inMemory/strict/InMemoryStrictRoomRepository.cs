using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using ABP.Application.Exceptions;

namespace ABP.Infrastructure.Repositories.InMemory.Strict;

/// <summary>
/// Strict in-memory room storage implementation.
/// </summary>
/// <remarks>
/// Soft means that if entry with this id already exists (for add/update) or missing (for remove), operation throws an exception.
/// </remarks>
public class InMemoryStrictRoomRepository : IRoomRepository {
    private readonly List<Room> _repository = [];

    /// <summary>
    /// Adds a new room to repository.
    /// </summary>
    /// <remarks>
    /// Duplicate of the room is stored. 
    /// </remarks>
    /// <param name="room">Room to be stored.</param>
    /// <exception cref="InvalidArgumentException">if the room with this id already exists.</exception>
    public async Task AddAsync(Room room)
    {
        if (_repository.Find(r => r.Id == room.Id) is null)
            _repository.Add(new (room));
        else throw new RepositoryException("Room with this id already exists.");
    }

    /// <summary>
    /// Removes room with `id` to repository.
    /// </summary>
    /// <param name="room">Id of room to be removed.</param>
    /// <exception cref="InvalidArgumentException">if the room with this id does not exist.</exception>
    public async Task RemoveAsync(string id)
    {
        if (_repository.RemoveAll(r => r.Id == id) == 0) 
            throw new RepositoryException("Room with this id does not exist.");
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
    /// <param name="room">Updated room data.</param>
    /// <exception cref="RepositoryException">if booking with this id does not exist.</exception>
    public async Task UpdateAsync(Room room)
    {
        if (_repository.Find(r => r.Id == room.Id) is null)
            throw new RepositoryException("Room with this id does not exist.");
        
        _repository[_repository.FindIndex(r => r.Id == room.Id)] = room;
    }

    /// <summary>
    /// Get all repository elements.
    /// </summary>
    /// <returns>Collection of repository elements.</returns>
    public async Task<IReadOnlyCollection<Room>> GetAllAsync() { 
        if (_repository.Count is 0) return [];
        var result = new List<Room>();
        result.AddRange(_repository.Select(r => new Room(r)));
        return result;
    }
}