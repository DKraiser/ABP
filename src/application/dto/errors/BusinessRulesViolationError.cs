using ABP.Domain.Result;

namespace ABP.Application.Dto.Errors;

public class BusinessRulesViolationError(IDictionary<string, string[]>? problems = null) : 
    Error("Request violates business rules.", problems) { }