using ABP.Application.Dto.Errors;
using ABP.Application.Dto.Infos;
using ABP.Application.Interfaces.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace ABP.Api.Controllers;

[ApiController]
[Route("reports")]
public class ReportsController : ControllerBase
{

    [HttpGet("utilizations")]
    [EndpointName("Room utilizations")]
    [EndpointSummary("Room usages over queried period")]
    [EndpointDescription("Returns utilization report for all rooms booked at least once in selected period")]
    [ProducesResponseType<IEnumerable<RoomUtilizationInfo>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/json")]
    public async Task<ActionResult<IEnumerable<RoomUtilizationInfo>>> GetUtilizationReportAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromServices] IReportHandler handler
    )
    {
        var result = await handler.GetRoomUtilizationsAsync(from, to);
        if (!result.IsSuccessful)
        {
            if (result.Error is DomainRulesViolationError)
                return UnprocessableEntity(ApiProblemDetails.DomainRulesViolation());
            else
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemDetails.InternalServerError(result)
                );
        }
        return Ok(result.Value);
    }

    [HttpGet("revenues")]
    [EndpointName("Room revenues")]
    [EndpointSummary("Room revenues over queried period")]
    [EndpointDescription("Returns revenue report for all rooms booked at least once in selected period")]
    [ProducesResponseType<IEnumerable<RoomUtilizationInfo>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/json")]
    public async Task<ActionResult<IEnumerable<RoomUtilizationInfo>>> GetRevenueReportAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromServices] IReportHandler handler
    )
    {
        var result = await handler.GetRoomRevenuesAsync(from, to);
        if (!result.IsSuccessful)
        {
            if (result.Error is DomainRulesViolationError)
                return UnprocessableEntity(ApiProblemDetails.DomainRulesViolation());
            else
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemDetails.InternalServerError(result)
                );
        }
        return Ok(result.Value);
    }
}
