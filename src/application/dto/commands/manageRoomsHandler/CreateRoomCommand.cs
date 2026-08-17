using ABP.Domain.Entities;

namespace ABP.Application.Dto.Commands.ManageRoomsHandler;

public record CreateRoomCommand(
    string Name = null!, 
    int Capacity = 0, 
    decimal BasePrice = 0, 
    List<Service>? Services = null
);