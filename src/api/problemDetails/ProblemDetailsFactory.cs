using ABP.Application.Dto.Errors;
using ABP.Domain.Result;
using Microsoft.AspNetCore.Mvc;

namespace ABP.Api.ProblemDetailz;

public static class ProblemDetailsFactory {
    public static ProblemDetails NotFound() =>
        new() {
            Type = nameof(NotFoundError),
            Title = "Object not found",
            Status = StatusCodes.Status404NotFound,
            Detail = "Object with requested id was not found."
        };

    public static ProblemDetails Conflict() =>
        new () {
            Type = nameof(ConflictError),
            Title = "Object exists",
            Status = StatusCodes.Status409Conflict,
            Detail = "This object already exists."
        };

    public static ProblemDetails DomainRulesViolation() =>
        new () {
            Type = nameof(DomainRulesViolationError),
            Title = "Domain rules violation",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = "Request violates domain rules."
        };

    public static ProblemDetails BusinessRulesViolation() =>
        new () {
            Type = nameof(DomainRulesViolationError),
            Title = "Business rules violation",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = "Request violates business rules."
        };

    public static ProblemDetails InternalServerError(Result result) =>
        new () {
            Type = result.Error!.GetType().Name,
            Title = "Internal server error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Unexpected server error occured."
        };
}