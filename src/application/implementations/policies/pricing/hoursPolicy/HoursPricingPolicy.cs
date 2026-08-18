using ABP.Application.Interfaces.Policies;
using ABP.Domain.Entities;

namespace ABP.Application.Implementations.Policies.Pricing.HoursPolicy;

/// <summary>
/// Calculates price basing on hours of booking and price periods.
/// </summary>
/// <remarks>
/// If reserved period is covered by no price periods, it will not be accounted. 
/// Such type of situations have to be controlled by <see>IBookingPolicy</see>.
/// </remarks>
public class HoursPricePolicy (IReadOnlyList<PricePeriod> pricePeriods) : IPricingPolicy {
    private readonly IReadOnlyList<PricePeriod> _pricePeriods = pricePeriods;

    public decimal CalculatePrice(Domain.Entities.Booking booking)
    {
        decimal roomPrice = 0;

        for (var date = booking.StartTime.Date;
             date <= booking.EndTime.Date;
             date = date.AddDays(1))
        {
            foreach (var period in _pricePeriods)
            {
                var periodStart = date.AddHours(period.StartTime.Hour);
                var periodEnd = date.AddHours(period.EndTime.Hour);

                var overlapStart = booking.StartTime > periodStart
                    ? booking.StartTime
                    : periodStart;

                var overlapEnd = booking.EndTime < periodEnd
                    ? booking.EndTime
                    : periodEnd;

                if (overlapStart >= overlapEnd)
                    continue;

                var hours = (decimal)(overlapEnd - overlapStart).TotalHours;

                roomPrice +=
                    hours *
                    booking.Room.BasePrice *
                    period.Multiplier;
            }
        }

        var servicesPrice = booking.RequestedServices
            .Sum(service => service.Price);

        return roomPrice + servicesPrice;
    }
}