using ABP.Domain.Result;

namespace ABP.Application.Dto.Errors.ManageRooms;

public class AddRoomError(IDictionary<string, string[]>? problems) : 
    Error("Failed to add a new room.", problems) { }