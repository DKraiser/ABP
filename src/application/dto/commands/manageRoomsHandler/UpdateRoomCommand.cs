using ABP.Domain.Entities;

namespace ABP.Application.Dto.Commands.ManageRoomsHandler;

public record UpdateRoomCommand(
    string Id, 
    string? NewName = null, 
    int NewCapacity = 0, 
    decimal NewBasePrice = -1, 
    List<Service>? NewServices = null,
    List<Service>? UpdatedServices = null,
    List<Service>? DeletedServices = null
);