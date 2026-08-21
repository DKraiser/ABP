namespace ABP.Application.Dto.Infos;

public record RoomUtilizationInfo(string RoomId, double BookedHours, double AverageBookedHoursPerDay);