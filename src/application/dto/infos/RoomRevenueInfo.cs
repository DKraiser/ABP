using ABP.Domain.Entities;

namespace ABP.Application.Dto.Infos;

public record RoomRevenueInfo(string RoomId, decimal Total, IDictionary<string, decimal> RevenuePerService);