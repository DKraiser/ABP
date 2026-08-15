using ABP.Application.Interfaces;
using ABP.Domain.Entities;

namespace ABP.Application.Implementations.Pricing.HoursPolicy;

/// <summary>
/// Calculates price basing on hours of booking and price periods. 
/// </summary>
/// <remarks>
/// In this implementation price periods are hardcoded, but it can be changed if configurations are introduced.
/// </remarks>
public class HoursPricePolicy : IPricingPolicy {
    
    public decimal CalculatePrice(Booking booking)
    {
        throw new NotImplementedException();
    }
}