using ABP.Api.Extensions;
using ABP.Api.ProblemDetailz;
using ABP.Api.Requests;
using ABP.Application.Dto.Commands.BookRoomsHandler;
using ABP.Application.Dto.Commands.ManageRoomsHandler;
using ABP.Application.Dto.Commands.SearchRoomsHandler;
using ABP.Application.Dto.Errors;
using ABP.Application.Dto.Infos;
using ABP.Application.Implementations.Handlers;
using ABP.Application.Implementations.Policies.Booking;
using ABP.Application.Implementations.Policies.Pricing.HoursPolicy;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using ABP.Infrastructure.Repositories.InMemory.Strict;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ========== Service registration ========== 

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IRoomRepository, InMemoryStrictRoomRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryStrictBookingRepository>();

builder.Services.AddScoped<IManageRoomsHandler, ManageRoomsHandler>();
builder.Services.AddScoped<ISearchRoomsHandler, SearchRoomsHandler>();
builder.Services.AddScoped<IBookRoomsHandler, BookRoomsHandler>();
builder.Services.AddScoped<IReportHandler, ReportHandler>();

builder.Services.AddBookingPolicies([
    new ForbiddenPeriodPolicy(new (23, 0), new (6, 0))
]);
builder.Services.AddPricingPolicies([
    new HoursPricePolicy([
        new (new (6, 0), new (9, 0), 0.9m),
        new (new (9, 0), new (12, 0), 1.0m),
        new (new (12, 0), new (14, 0), 1.15m),
        new (new (14, 0), new (18, 0), 1.0m),
        new (new (18, 0), new (23, 0), 0.8m)
    ])
]);

var app = builder.Build();

// ========== Configuration ==========  

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{ 
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => {
        o.Metadata = new Dictionary<string, string>(){
            { "Title", "Booking API" },
            { "Description", "Test task for ABP job application." }
        };
    });
}

if (app.Environment.IsProduction()) {
    app.UseExceptionHandler();
}

// ========== Seed ==========  

await using (var scope = app.Services.CreateAsyncScope()) {
    var handler = scope.ServiceProvider.GetRequiredService<IManageRoomsHandler>();
    await handler.CreateAsync(new CreateRoomCommand(
        "Room A", 50, 2000, [
            new ("Projector", 500),
            new ("WiFi", 300),
            new ("Sound", 700)
        ]
    ));
    await handler.CreateAsync(new CreateRoomCommand(
        "Room B", 100, 3500, [
            new ("Projector", 500),
            new ("WiFi", 300),
            new ("Sound", 700)
        ]
    ));
    await handler.CreateAsync(new CreateRoomCommand(
        "Room C", 30, 1500, [
            new ("Projector", 500),
            new ("WiFi", 300),
            new ("Sound", 700)
        ]
    ));
}

// ========== Room management endpoints ==========  

var roomsGroup = app.MapGroup("/rooms");

roomsGroup.MapGet("/{id}", async (string id, IManageRoomsHandler handler) => { 
    var result = await handler.FindAsync(new FindRoomCommand(id));
    if (!result.IsSuccessful) {
        if (result.Error is NotFoundError)
            return Results.NotFound(ProblemDetailsFactory.NotFound());
        else 
            return Results.InternalServerError(ProblemDetailsFactory.InternalServerError(result));
    }
    
    return Results.Ok<RoomInfo>(result.Value);
})
.WithName("Get room info")
.WithSummary("Gets information about room with this id.")
.WithDescription("Returns `RoomInfo` object if room with requested id exists.")
.Produces<RoomInfo>(StatusCodes.Status200OK, "application/json")
.Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/json");

roomsGroup.MapGet("/", async (IManageRoomsHandler handler) => { 
    var result = await handler.ListAllRoomsAsync();

    if (!result.IsSuccessful)
        return Results.InternalServerError(ProblemDetailsFactory.InternalServerError(result));

    if (result.Value!.Count is 0)
        return Results.NoContent();

    return Results.Ok(result.Value);
})
.WithName("List rooms")
.WithSummary("Lists all existing rooms.")
.WithDescription("Returns list of `RoomInfo` objects. This method is should not throw errors.")
.Produces<IReadOnlyList<RoomInfo>>(StatusCodes.Status200OK, "application/json")
.Produces(StatusCodes.Status204NoContent);

roomsGroup.MapPost("/", async ([FromBody] CreateRoomRequest request, IManageRoomsHandler handler) => {
    var result = await handler.CreateAsync(
        new CreateRoomCommand(
            request.Name,
            request.Capacity,
            request.BasePrice,
            [.. request.AvailableServices.Select<ServiceRequestNoId, Service>(r => new Service(r.Name, r.Price))]
        )
    );

    if (!result.IsSuccessful) {
        if (result.Error is DomainRulesViolationError)
            return Results.UnprocessableEntity(ProblemDetailsFactory.DomainRulesViolation());
        else if (result.Error is ConflictError)
            return Results.Conflict(ProblemDetailsFactory.Conflict());
        else 
            return Results.InternalServerError(ProblemDetailsFactory.InternalServerError(result));
    }

    return Results.CreatedAtRoute<string>("Get room info", new { id = result.Value }, result.Value);
})
.WithName("Create a new room")
.WithSummary("Creates a new room with requested data.")
.WithDescription("Creates and stores a new room with data provided in request." +
    "If request was successful, new room's id is returned." +
    "Fails if request violate business/domain rules or causes a conflict.")
.Accepts<CreateRoomRequest>("application/json")
.Produces<string>(StatusCodes.Status201Created)
.Produces<ProblemDetails>(StatusCodes.Status409Conflict)
.Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

roomsGroup.MapPut("/{id}", async (string id, [FromBody] UpdateRoomRequest request, IManageRoomsHandler handler) => {
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

    if (!result.IsSuccessful) { 
        if (result.Error is NotFoundError)
            return Results.NotFound(ProblemDetailsFactory.NotFound());
        else if (result.Error is DomainRulesViolationError)
            return Results.UnprocessableEntity(ProblemDetailsFactory.DomainRulesViolation());
        else 
            return Results.InternalServerError(ProblemDetailsFactory.InternalServerError(result));
    }

    return Results.NoContent();
})
.WithName("Update room data")
.WithSummary("Updates data of the requested room.")
.WithDescription("Updates and stores the room with data provided in request." +
    "If request was successful, Ok is returned." +
    "Fails if request violate business/domain rules or room does not exist.")
.Accepts<UpdateRoomRequest>("application/json")
.Produces(StatusCodes.Status204NoContent)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

roomsGroup.MapDelete("/{id}", async (string id, IManageRoomsHandler handler) => {
    var result = await handler.DeleteAsync(new DeleteRoomCommand(id));
    if (!result.IsSuccessful) {
        if (result.Error is NotFoundError)
            return Results.NotFound(ProblemDetailsFactory.NotFound());
        else 
            return Results.InternalServerError(ProblemDetailsFactory.InternalServerError(result));
    }

    return Results.NoContent();
})
.WithName("Delete a room")
.WithSummary("Deletes a room with the specified id.")
.WithDescription("Deletes a room with the specified id." +
    "If succeeds, returns Ok, else returns NotFound")
.Produces(StatusCodes.Status204NoContent)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound);

// ========== Booking endpoints ==========  

var bookingsGroup = app.MapGroup("/bookings");

bookingsGroup.MapGet("/spare", async (
    [FromQuery] DateOnly date,
    [FromQuery] TimeOnly startTime,
    [FromQuery] TimeOnly endTime,
    [FromQuery] int minimalCapacity,
    [FromServices] ISearchRoomsHandler handler) => 
{
    var result = await handler.SearchRoomsAsync(
        new SearchRoomsCommand(date, startTime, endTime, minimalCapacity)
    );

    if (!result.IsSuccessful) 
        if (result.Error is DomainRulesViolationError)
            return Results.UnprocessableEntity(ProblemDetailsFactory.DomainRulesViolation());

        else return Results.InternalServerError(ProblemDetailsFactory.InternalServerError(result));
    
    if (!result.Value!.Any())
        return Results.NoContent();

    return Results.Ok(result.Value);
})
.WithName("Search spare rooms")
.WithSummary("Searches spare rooms matching request criteria.")
.WithDescription("Returns list of room info objects representing all " +
    "rooms that are spare and match request criteria."
)
.Produces<IReadOnlyList<RoomInfo>>(StatusCodes.Status200OK, "application/json")
.Produces(StatusCodes.Status204NoContent);

bookingsGroup.MapPost("/book", async ([FromBody] BookRoomRequest request, IBookRoomsHandler handler) => {
    var result = await handler.BookRoomAsync(
        new BookRoomCommand(request.RoomId, request.Date, request.StartTime, request.EndTime, request.RequestedServiceIds)
    );

    if (!result.IsSuccessful) { 
        if (result.Error is NotFoundError)
            return Results.NotFound(ProblemDetailsFactory.NotFound());
        else if (result.Error is DomainRulesViolationError)
            return Results.UnprocessableEntity(ProblemDetailsFactory.DomainRulesViolation());
        else if (result.Error is BusinessRulesViolationError)
            return Results.UnprocessableEntity(ProblemDetailsFactory.BusinessRulesViolation());
        else if (result.Error is ConflictError)
            return Results.Conflict(ProblemDetailsFactory.Conflict());
        else
            return Results.InternalServerError(ProblemDetailsFactory.InternalServerError(result));
    }

    return Results.Ok(result.Value);
})
.WithName("Book the room")
.WithSummary("Books the room with requested services")
.WithDescription("Creates, validates and stores a new booking.")
.Accepts<BookRoomRequest>("application/json")
.Produces<BookingConfirmationInfo>(StatusCodes.Status200OK, "application/json")
.Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/json")
.Produces<ProblemDetails>(StatusCodes.Status409Conflict, "application/json")
.Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/json");

var reportsGroup = app.MapGroup("/reports");
reportsGroup.MapGet("/utilization", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, IReportHandler handler) => {
    return Results.Ok(await handler.GetRoomUtilizationsAsync(from, to));
})
.WithName("Room utilization")
.WithSummary("Room usage over queried period")
.WithDescription("Returns utilization report for all rooms registered right now.")
.Produces<IReadOnlyList<RoomUtilizationInfo>>(StatusCodes.Status200OK, "application/json");
app.Run();