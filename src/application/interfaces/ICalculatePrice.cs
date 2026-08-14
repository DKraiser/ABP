using ABP.Domain.Entities;

namespace ABP.Application.Interfaces;

/// <summary>
/// Mechanism calculating price of booking according to concrete pricing policy.
/// </summary>
public interface ICalculatePrice {
    /// <summary>
    /// Calculate price booking.
    /// </summary>
    /// <param name="booking">Booking price of which to calculate.</param>
    /// <returns>Price of booking.</returns>
    public decimal Calculate(Booking booking);
}