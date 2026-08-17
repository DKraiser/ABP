using ABP.Domain.Result;

namespace ABP.Application.Dto.Errors;

public class InfrastructureError(IDictionary<string, string[]>? problems = null) : 
    Error("Request violates infrastructure rules.", problems) { }