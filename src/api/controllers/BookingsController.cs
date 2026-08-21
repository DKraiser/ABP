using ABP.Application.Dto.Infos;
using ABP.Application.Dto.Commands.SearchRoomsHandler;
using ABP.Application.Interfaces.Handlers;
using Microsoft.AspNetCore.Mvc;
using ABP.Application.Dto.Errors;
using ABP.Api.Requests;
using ABP.Application.Dto.Commands.BookRoomsHandler;

namespace ABP.Api.Controllers;

[ApiController]
[Route("bookings")]
public class BookingsController : ControllerBase
{

    [HttpGet("available-rooms")]
    [EndpointName("Search available rooms")]
    [EndpointSummary("Searches available rooms matching user's criteria.")]
    [EndpointDescription("Returns list of room info objects representing all " +
        "rooms that are available and match user's criteria."
    )]
    [ProducesResponseType<IEnumerable<RoomInfo>>(StatusCodes.Status200OK, "application/json")]
    public async Task<ActionResult<IEnumerable<RoomInfo>>> SearchAvailableAsync(
        [FromQuery] DateOnly date,
        [FromQuery] TimeOnly startTime,
        [FromQuery] TimeOnly endTime,
        [FromQuery] int minimalCapacity,
        [FromServices] ISearchAvailableRoomsHandler handler
    )
    {
        var result = await handler.SearchAvailableAsync(
            new SearchAvailableRoomsCommand(date, startTime, endTime, minimalCapacity)
        );

        if (!result.IsSuccessful)
            if (result.Error is DomainRulesViolationError)
                return UnprocessableEntity(ApiProblemDetails.DomainRulesViolation());

            else
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemDetails.InternalServerError(result)
                );

        return Ok(result.Value);
    }

    [HttpPost("")]
    [EndpointName("Book the room")]
    [EndpointSummary("Books the room with requested services")]
    [EndpointDescription("Creates, validates and stores a new booking.")]
    [Consumes(typeof(BookRoomRequest), "application/json")]
    [ProducesResponseType<BookingConfirmationInfo>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/json")]
    public async Task<ActionResult<BookingConfirmationInfo>> BookAsync(
        [FromBody] BookRoomRequest request,
        [FromServices] IBookRoomsHandler handler
    )
    {
        var result = await handler.BookRoomAsync(
            new BookRoomCommand(
                request.RoomId,
                request.Date,
                request.StartTime,
                request.EndTime,
                request.RequestedServiceIds
            )
        );

        if (!result.IsSuccessful)
        {
            if (result.Error is NotFoundError)
                return NotFound(ApiProblemDetails.NotFound());
            else if (result.Error is DomainRulesViolationError)
                return UnprocessableEntity(ApiProblemDetails.DomainRulesViolation());
            else if (result.Error is BusinessRulesViolationError)
                return UnprocessableEntity(ApiProblemDetails.BusinessRulesViolation());
            else if (result.Error is ConflictError)
                return Conflict(ApiProblemDetails.Conflict());
            else
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemDetails.InternalServerError(result)
                );
        }

        return Ok(result.Value);
    }
}