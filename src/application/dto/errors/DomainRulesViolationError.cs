using ABP.Domain.Result;

namespace ABP.Application.Dto.Errors;

public class DomainRulesViolationError(IDictionary<string, string[]>? problems = null) :
    Error("Request violates domain rules.", problems)
{ }