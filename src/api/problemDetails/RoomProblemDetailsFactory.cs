using ABP.Application.Dto.Errors;
using ABP.Domain.Result;
using Microsoft.AspNetCore.Mvc;

namespace ABP.Api.ProblemDetailz;

public static class RoomProblemDetailsFactory {
    public static ProblemDetails NotFound() =>
        new() {
            Type = nameof(NotFoundError),
            Title = "Room not found",
            Status = StatusCodes.Status404NotFound,
            Detail = "Room with requested id was not found."
        };

    public static ProblemDetails Conflict() =>
        new () {
            Type = nameof(ConflictError),
            Title = "Duplication error",
            Status = StatusCodes.Status409Conflict,
            Detail = "Room with this id already exists."
        };

    public static ProblemDetails DomainRulesViolation() =>
        new () {
            Type = nameof(DomainRulesViolationError),
            Title = "Domain rules violation",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = "Request violates domain rules."
        };

    public static ProblemDetails InternalServerError(Result result) =>
        new () {
            Type = result.Error.GetType().Name,
            Title = "Internal server error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Unexpected server error occured."
        };
}