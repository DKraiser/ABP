namespace ABP.Api.Requests;

public record BookRoomRequest(string RoomId, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, IReadOnlyList<string> RequestedServiceIds);