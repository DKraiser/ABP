namespace ABP.Application.Dto;

public record BookingConfirmation (string Id, decimal Price, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);