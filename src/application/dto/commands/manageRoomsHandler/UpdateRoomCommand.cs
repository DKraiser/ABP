using ABP.Domain.Entities;

namespace ABP.Application.Dto.Commands.ManageRoomsHandler;

public record UpdateRoomCommand(
    string Id, 
    string? NewName = null, 
    int NewCapacity = 0, 
    decimal NewBasePrice = -1, 
    IEnumerable<Service>? NewServices = null,
    IEnumerable<Service>? UpdatedServices = null,
    IEnumerable<Service>? DeletedServices = null
);