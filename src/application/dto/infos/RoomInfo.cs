using ABP.Domain.Entities;

namespace ABP.Application.Dto.Infos;

public record RoomInfo(string Id, string Name, int Capacity, decimal BasePrice, IReadOnlyList<Service> AvailableServices);