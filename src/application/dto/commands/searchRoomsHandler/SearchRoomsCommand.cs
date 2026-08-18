using ABP.Domain.Entities;

namespace ABP.Application.Dto.Commands.SearchRoomsHandler;

public record SearchRoomsCommand(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, int MinimalCapacity);