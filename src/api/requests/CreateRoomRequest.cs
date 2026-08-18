namespace ABP.Api.Requests;

public record CreateRoomRequest(string Name, int Capacity, decimal BasePrice, IReadOnlyList<ServiceRequestNoId> AvailableServices);