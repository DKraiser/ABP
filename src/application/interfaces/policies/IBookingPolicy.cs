using ABP.Domain.Entities;

namespace ABP.Application.Interfaces.Policies;

/// <summary>
/// Mechanism checking if particular bookings are allowed.
/// </summary>
public interface IBookingPolicy
{
    /// <summary>
    /// Check if such type of booking is allowed.
    /// </summary>
    /// <param name="booking">Booking to check.</param>
    public bool IsAllowed(Booking booking);
}