using ABP.Domain.Entities;

namespace ABP.Application.Dto.Commands.ManageRoomsHandler;

public record UpdateRoomCommand(
    string Id,
    string? NewName = null,
    int NewCapacity = 0,
    decimal NewBasePrice = 0,
    IReadOnlyList<Service>? NewServices = null,
    IReadOnlyList<Service>? UpdatedServices = null,
    IReadOnlyList<string>? RemovedServices = null
);