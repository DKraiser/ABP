namespace ABP.Application.Dto.Infos;

public record BookingConfirmationInfo(string Id, decimal Price, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);