using ABP.Domain.Entities;

namespace ABP.Application.Dto.Commands.BookRoomsHandler;

public record BookRoomCommand(string RoomId, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, List<string> ServiceIds);