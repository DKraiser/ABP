using ABP.Api.Extensions;
using ABP.Application.Dto.Infos;
using ABP.Application.Implementations.Handlers;
using ABP.Application.Implementations.Policies.Booking;
using ABP.Application.Implementations.Policies.Pricing.HoursPolicy;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Repositories;
using ABP.Infrastructure.Repositories.InMemory.Strict;
using Microsoft.Extensions.Logging.Configuration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IRoomRepository, InMemoryStrictRoomRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryStrictBookingRepository>();

builder.Services.AddScoped<IManageRoomsHandler, ManageRoomsHandler>();
builder.Services.AddScoped<ISearchRoomsHandler, SearchRoomsHandler>();
builder.Services.AddScoped<IBookRoomsHandler, BookRoomsHandler>();

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
            { "Description", "Test task for ABP job application." },
        };
    });
}

if (app.Environment.IsProduction()) {
    app.UseExceptionHandler();
}

app.MapGet("/rooms/list", async (IManageRoomsHandler handler) => { 
    var result = await handler.ListAllRoomsAsync();
    if (result?.Value?.Count is 0)
        return Results.NoContent();

    return Results.Ok(result);
})
.WithName("List rooms")
.WithSummary("Lists all existing rooms.")
.WithDescription("Returns list of `RoomInfo` objects. This method is should not throw errors.")
.Produces<IReadOnlyList<RoomInfo>>(StatusCodes.Status200OK, "application/json")
.Produces(StatusCodes.Status204NoContent);

app.Run();