using ABP.Domain.Result;

namespace ABP.Application.Dto.Errors;

public class NotFoundError(IDictionary<string, string[]>? problems = null) :
    Error("Requested object was not found.", problems)
{ }