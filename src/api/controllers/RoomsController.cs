using ABP.Application.Dto.Commands.ManageRoomsHandler;
using ABP.Application.Dto.Infos;
using ABP.Application.Dto.Errors;
using ABP.Application.Interfaces.Handlers;
using Microsoft.AspNetCore.Mvc;
using ABP.Api.Requests;
using ABP.Domain.Entities;

namespace ABP.Api.Controllers;

[ApiController]
[Route("rooms")]
public class RoomsController : ControllerBase
{

    [HttpGet("")]
    [EndpointName("List rooms")]
    [EndpointSummary("Lists all existing rooms.")]
    [EndpointDescription("Returns list of `RoomInfo` objects. This method is should not throw errors.")]
    [ProducesResponseType<IEnumerable<RoomInfo>>(StatusCodes.Status200OK, "application/json")]
    public async Task<ActionResult<IEnumerable<RoomInfo>>> ListAllAsync(
        [FromServices] IManageRoomsHandler handler
    )
    {
        var result = await handler.ListAllRoomsAsync();

        if (!result.IsSuccessful)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiProblemDetails.InternalServerError(result)
            );

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    [EndpointName("Get room info")]
    [EndpointSummary("Gets information about room with this id.")]
    [EndpointDescription("Returns `RoomInfo` object if room with requested id exists.")]
    [ProducesResponseType<RoomInfo>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/json")]
    public async Task<ActionResult<RoomInfo?>> GetRoomByIdAsync(
        [FromRoute] string id,
        [FromServices] IManageRoomsHandler handler
    )
    {
        var result = await handler.FindAsync(new FindRoomCommand(id));
        if (!result.IsSuccessful)
        {
            if (result.Error is NotFoundError)
                return NotFound(ApiProblemDetails.NotFound());
            else
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemDetails.InternalServerError(result)
                );
        }

        return Ok(result.Value);
    }

    [HttpPost("")]
    [EndpointName("Create a new room")]
    [EndpointSummary("Creates a new room with requested data.")]
    [EndpointDescription("Creates and stores a new room with data provided in request." +
        "If request was successful, new room's id is returned." +
        "Fails if request violate business/domain rules or causes a conflict."
    )]
    [Consumes(typeof(CreateRoomRequest), "application/json")]
    [ProducesResponseType<string>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<string>> CreateRoomAsync(
        [FromBody] CreateRoomRequest request,
        [FromServices] IManageRoomsHandler handler
    )
    {
        var result = await handler.CreateAsync(
            new CreateRoomCommand(
                request.Name,
                request.Capacity,
                request.BasePrice,
                [.. request.AvailableServices.Select<ServiceRequestNoId, Service>(r => new Service(r.Name, r.Price))]
            )
        );

        if (!result.IsSuccessful)
        {
            if (result.Error is DomainRulesViolationError)
                return UnprocessableEntity(ApiProblemDetails.DomainRulesViolation());
            else if (result.Error is ConflictError)
                return Conflict(ApiProblemDetails.Conflict());
            else
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemDetails.InternalServerError(result)
                );
        }

        return CreatedAtRoute("Get room info", new { id = result.Value }, result.Value);
    }

    [HttpPut("{id}")]
    [EndpointName("Update room data")]
    [EndpointSummary("Updates data of the requested room.")]
    [EndpointDescription("Updates and stores the room with data provided in request." +
        "If request was successful, Ok is returned." +
        "Fails if request violate business/domain rules or room does not exist."
    )]
    [Consumes(typeof(UpdateRoomRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateAsync(
        [FromRoute] string id,
        [FromBody] UpdateRoomRequest request,
        [FromServices] IManageRoomsHandler handler
    )
    {
        var result = await handler.UpdateAsync(
            new UpdateRoomCommand(
                id,
                request.NewName,
                request.NewCapacity,
                request.NewBasePrice,
                request.NewServices?.Select<ServiceRequestNoId, Service>(r => new Service(r.Name, r.Price)).ToList(),
                request.UpdatedServices?.Select<ServiceRequestId, Service>(r => new Service(r.Id, r.Name, r.Price)).ToList(),
                request.RemovedServices
            )
        );

        if (!result.IsSuccessful)
        {
            if (result.Error is NotFoundError)
                return NotFound(ApiProblemDetails.NotFound());
            else if (result.Error is DomainRulesViolationError)
                return UnprocessableEntity(ApiProblemDetails.DomainRulesViolation());
            else
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemDetails.InternalServerError(result)
                );
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [EndpointName("Delete a room")]
    [EndpointSummary("Deletes a room with the specified id.")]
    [EndpointDescription("Deletes a room with the specified id." +
        "If succeeds, returns Ok, else returns NotFound"
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(
        [FromRoute] string id,
        [FromServices] IManageRoomsHandler handler
    )
    {
        var result = await handler.DeleteAsync(new DeleteRoomCommand(id));
        if (!result.IsSuccessful)
        {
            if (result.Error is NotFoundError)
                return NotFound(ApiProblemDetails.NotFound());
            else
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemDetails.InternalServerError(result)
                );
        }

        return NoContent();
    }
}