using ABP.Domain.Result;

namespace ABP.Application.Dto.Errors;

public class ConflictError(IDictionary<string, string[]>? problems = null) :
    Error("Request causes a conflict.", problems)
{ }