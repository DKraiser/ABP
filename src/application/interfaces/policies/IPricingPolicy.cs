using ABP.Domain.Entities;

namespace ABP.Application.Interfaces.Policies;

/// <summary>
/// Mechanism calculating price of booking according to concrete pricing policy.
/// </summary>
public interface IPricingPolicy {
    /// <summary>
    /// Calculate price booking.
    /// </summary>
    /// <param name="booking">Booking price of which to calculate.</param>
    /// <returns>Price of booking.</returns>
    public decimal CalculatePrice(Booking booking);
}