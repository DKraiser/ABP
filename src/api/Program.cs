using ABP.Api.Extensions;
using ABP.Api.Options.InitialPolicies;
using ABP.Api.Options.InitialRooms;
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

// ========== Options registration ========== 

builder.Services.AddOptions<List<RoomOptions>>()
    .Bind(builder.Configuration.GetSection(RoomOptions.ConfigurationSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<List<ForbiddenPeriodPolicyOptions>>()
    .Bind(builder.Configuration.GetSection(ForbiddenPeriodPolicyOptions.ConfigurationSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<List<HoursPricePeriodPolicyOptions>>()
    .Bind(builder.Configuration.GetSection(HoursPricePeriodPolicyOptions.ConfigurationSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ========== Service registration ========== 

builder.Services.AddCors();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IRoomRepository, InMemoryStrictRoomRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryStrictBookingRepository>();

builder.Services.AddScoped<IManageRoomsHandler, ManageRoomsHandler>();
builder.Services.AddScoped<ISearchAvailableRoomsHandler, SearchAvailableRoomsHandler>();
builder.Services.AddScoped<IBookRoomsHandler, BookRoomsHandler>();
builder.Services.AddScoped<IReportHandler, ReportHandler>();

var forbiddenPeriodPoliciesConfigSection = builder.Configuration
    .GetSection(ForbiddenPeriodPolicyOptions.ConfigurationSectionName);
var hoursPricingPoliciesConfigSection = builder.Configuration
    .GetSection(HoursPricePeriodPolicyOptions.ConfigurationSectionName);

builder.Services.AddForbiddenPeriodBookingPoliciesFromConfiguration(forbiddenPeriodPoliciesConfigSection);
builder.Services.AddHoursPricePoliciesFromConfiguration(hoursPricingPoliciesConfigSection);

var app = builder.Build();

// ========== Configuration ==========  

app.MapControllers();
app.UseCors(b => {
    b.AllowAnyHeader();
    b.AllowAnyMethod();
    b.AllowAnyOrigin();
});

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
        o.Metadata = new Dictionary<string, string>(){
            { "Title", "Booking API" },
            { "Description", "Test task for ABP job application." }
        };
    });
}

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler(handler => {
        handler.Run(async context => {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        });
    });
}

// ========== Healthchecks ==========  

app.MapHealthChecks("/health/live", new () {
    Predicate = _ => false
}).ExcludeFromDescription();
app.MapHealthChecks("/health/ready", new () {
    Predicate = _ => false
}).ExcludeFromDescription();

// ========== Seed ==========  

var initialRooms = app.Configuration
    .GetSection(RoomOptions.ConfigurationSectionName)
    .Get<IReadOnlyList<RoomOptions>>() ?? [];
await using (var scope = app.Services.CreateAsyncScope())
{
    var handler = scope.ServiceProvider.GetRequiredService<IManageRoomsHandler>();

    foreach (var room in initialRooms ?? [])
    {
        await handler.CreateAsync(new(
            room.Name, 
            room.Capacity, 
            room.BasePrice, 
            room.AvailableServices?
                .Select(so => new Service(so.Name, so.Price))
                .ToList() ?? []
        ));
    }
}

app.Run();