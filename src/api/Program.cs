using ABP.Api;
using ABP.Api.Extensions;
using ABP.Api.Options.InitialPolicies;
using ABP.Api.Options.InitialRooms;
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

// ========== Options registration ========== 

#region options
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
#endregion

// ========== Service registration ========== 

#region services

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSingleton<IRoomRepository, InMemoryStrictRoomRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryStrictBookingRepository>();

builder.Services.AddScoped<IManageRoomsHandler, ManageRoomsHandler>();
builder.Services.AddScoped<ISearchAvailableRoomsHandler, SearchAvailableRoomsHandler>();
builder.Services.AddScoped<IBookRoomsHandler, BookRoomsHandler>();
builder.Services.AddScoped<IReportHandler, ReportHandler>();

var forbiddenPeriodPolicies = builder.Configuration
    .GetSection(ForbiddenPeriodPolicyOptions.ConfigurationSectionName)
    .Get<IReadOnlyList<ForbiddenPeriodPolicyOptions>>();
var hoursPricingPolicies = builder.Configuration
    .GetSection(HoursPricePeriodPolicyOptions.ConfigurationSectionName)
    .Get<IReadOnlyList<HoursPricePeriodPolicyOptions>>();

builder.Services.AddBookingPolicies(
    forbiddenPeriodPolicies?.Select(p => new ForbiddenPeriodPolicy(
        new(p.StartHour, p.StartMinute),
        new(p.EndHour, p.EndMinute)
    )).ToList() ?? []
);
builder.Services.AddPricingPolicies([
    new HoursPricePolicy(
        hoursPricingPolicies?.Select(p => new PricePeriod(
            new (p.StartHour, p.StartMinute),
            new (p.EndHour, p.EndMinute),
            Convert.ToDecimal(p.Multiplier)
        )
    ).ToList() ?? [])
]);

#endregion

var app = builder.Build();

// ========== Configuration ==========  

#region configuration

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
    app.UseExceptionHandler();
}

app.MapControllers();

#endregion

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
            room.Name, room.Capacity, room.BasePrice, room.AvailableServices?.Select(so => new Service(so.Name, so.Price)).ToList() ?? []
        ));
    }
}

app.Run();