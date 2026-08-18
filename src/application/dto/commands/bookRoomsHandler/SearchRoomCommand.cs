using ABP.Domain.Entities;

namespace ABP.Application.Dto.Commands.BookRoomsHandler;

public record SearchRoomsCommand(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, int MinimalCapacity);