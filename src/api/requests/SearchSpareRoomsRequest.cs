namespace ABP.Api.Requests;

public record SearchSpareRoomsRequest(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, int MinimalCapacity);
