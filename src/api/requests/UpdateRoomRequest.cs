namespace ABP.Api.Requests;

public record UpdateRoomRequest (
    string? NewName = null, 
    int NewCapacity = 0, 
    decimal NewBasePrice = 0, 
    IReadOnlyList<ServiceRequestNoId>? NewServices = null,
    IReadOnlyList<ServiceRequestId>? UpdatedServices = null,
    IReadOnlyList<string>? RemovedServices = null
);