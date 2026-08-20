using ABP.Application.Dto.Commands.BookRoomsHandler;
using ABP.Application.Dto.Errors;
using ABP.Application.Dto.Infos;
using ABP.Application.Exceptions;
using ABP.Application.Interfaces.Handlers;
using ABP.Application.Interfaces.Policies;
using ABP.Application.Interfaces.Repositories;
using ABP.Domain.Entities;
using ABP.Domain.Exceptions;
using ABP.Domain.Result;

namespace ABP.Application.Implementations.Handlers;

public class BookRoomsHandler(
    IBookingRepository bookingRepository, 
    IRoomRepository roomRepository, 
    IReadOnlyList<IBookingPolicy> bookingPolicies,
    IReadOnlyList<IPricingPolicy> pricingPolicies
) : IBookRoomsHandler
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IRoomRepository _roomRepository = roomRepository;
    private readonly IReadOnlyList<IBookingPolicy> _bookingPolicies = bookingPolicies;
    private readonly IReadOnlyList<IPricingPolicy> _pricingPolicies = pricingPolicies;
    
    /// <summary>
    /// Books the room if booking does not violate anything.
    /// </summary>
    /// <param name="command">Data needed to create booking.</param>
    /// <returns>`Result<string>.Success()` with `BookingConfirmationInfo` if created successfully.</returns>
    /// <returns>`Result<string>.Failure(DomainRulesViolationError)` if data violate domain rules.</returns>
    /// <returns>`Result<string>.Failure(BusinessRulesViolationError)` if booking is forbidden by some booking policies.</returns>
    /// <returns>`Result<string>.Failure(NotFoundError)` if room or service with this id does not exist.</returns>
    /// <returns>`Result<string>.Failure(ConflictError)` if booking on that time already exists.</returns>
    public async Task<Result<BookingConfirmationInfo>> BookRoomAsync(
        BookRoomCommand command)
    {
        // Check if requested room exists.
        var room = await _roomRepository.FindByIdAsync(command.RoomId);

        if (room is null) {
            // If room was not found, return a failure.
            var notFoundProblems = new Dictionary<string, string[]> {
                ["Room"] = ["Room with this id does not exist."]
            };

            return Result<BookingConfirmationInfo>.Failure(new NotFoundError(notFoundProblems));
        }

        var startTime = command.Date.ToDateTime(command.StartTime);
        var endTime = command.Date.ToDateTime(command.EndTime);

        // Check if all requested services exist.
        var services = new List<Service>();
        var notExistingServices = new List<String>();

        foreach (var id in command.RequestedServiceIds ?? [])
        {
            var service = room.AvailableServices
                .FirstOrDefault(s => s.Id == id);

            if (service is null)         
            {   
                notExistingServices.Add($"Service '{id}' is not available in this room.");
                continue;
            }

            services.Add(service);
        }

        // If some of requested services were not found, 
        // return the corresponding error.
        if (notExistingServices.Count is not 0) {
            var problems = new Dictionary<string, string[]> {
                ["Service"] = [.. notExistingServices]
            };

            return Result<BookingConfirmationInfo>.Failure(
                new NotFoundError(problems));
        }

        // Create booking if reqeust does not violate domain rules. 
        Booking booking;

        try
        {
            booking = new Booking(room, startTime, endTime, services);
        }
        catch (DomainRulesViolationException exception)
        {
            var problems = new Dictionary<string, string[]>
            {
                ["Booking"] = [$"Failed to create a booking. {exception.Message}"]
            };

            return Result<BookingConfirmationInfo>.Failure(
                new DomainRulesViolationError(problems));
        }

        // Check booking policies
        foreach (var policy in _bookingPolicies)
        {
            if (!policy.IsAllowed(booking))
            {
                var problems = new Dictionary<string, string[]>
                {
                    ["Booking"] = ["The booking is not allowed."]
                };

                return Result<BookingConfirmationInfo>.Failure(
                    new BusinessRulesViolationError(problems));
            }
        }

        // Check if booking does not conflict with other ones.
        if ((await _bookingRepository.GetAllAsync()).Where(b => b.Overlaps(booking)).Any()) {
            var problems = new Dictionary<string, string[]>
            {
                ["Booking"] = ["The booking overlaps an already existing one."]
            };

            return Result<BookingConfirmationInfo>.Failure(new ConflictError (problems));
        }

        // Calculate price.
        decimal price = 0;

        foreach (var policy in _pricingPolicies)
            price += policy.CalculatePrice(booking);

        booking.Price = price;

        // Store booking.
        await _bookingRepository.AddAsync(booking);

        // If all is ok, return confirmation.
        return Result<BookingConfirmationInfo>.Success(
            new BookingConfirmationInfo(
                booking.Id,
                price,
                command.Date,
                command.StartTime,
                command.EndTime));
    }
}