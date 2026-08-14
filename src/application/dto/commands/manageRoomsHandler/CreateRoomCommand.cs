using ABP.Domain.Entities;

namespace ABP.Application.Dto.Commands.ManageRoomsHandler;

public record CreateRoomCommand(
    string Id, 
    string? Name = null, 
    int NewCapacity = 0, 
    decimal NewBasePrice = -1, 
    IEnumerable<Service>? Services = null
);