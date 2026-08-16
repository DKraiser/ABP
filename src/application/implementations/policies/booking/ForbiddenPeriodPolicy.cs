using ABP.Application.Interfaces.Policies;
using ABP.Domain.Entities;

namespace ABP.Application.Implementations.Policies.Booking;

/// <summary>
/// This policy is responsible for periods of day when bookings are restricted (e.g. night time, cleanings).
/// </summary>
/// <remarks>
/// If forbidden period supposes date change, it have to be splitted at 24th hour.
/// </remarks>
public class ForbiddenPeriodPolicy : IBookingPolicy
{
    private readonly TimeOnly _forbiddenPeriodStart;
    private readonly TimeOnly _forbiddenPeriodEnd;

    public ForbiddenPeriodPolicy (TimeOnly forbiddenPeriodStart, TimeOnly forbiddenPeriodEnd) { 
        if (forbiddenPeriodStart == forbiddenPeriodEnd)
            throw new ArgumentException("Period duration must be positive.");
        
        _forbiddenPeriodStart = forbiddenPeriodStart;
        _forbiddenPeriodEnd = forbiddenPeriodEnd;
    }

    public bool IsAllowed(Domain.Entities.Booking booking)
    { 
        if (_forbiddenPeriodStart < _forbiddenPeriodEnd) {
            if (booking.StartTime.Date == booking.EndTime.Date) { 
                return booking.StartTime.TimeOfDay >= _forbiddenPeriodEnd.ToTimeSpan() || 
                    booking.EndTime.TimeOfDay <= _forbiddenPeriodStart.ToTimeSpan();
            }
            else if (booking.StartTime.Date.AddDays(1) == booking.EndTime.Date) { 
                return booking.StartTime.TimeOfDay >= _forbiddenPeriodEnd.ToTimeSpan() &&
                    booking.EndTime.TimeOfDay <= _forbiddenPeriodStart.ToTimeSpan(); 
            }
            else return false;
        }
        else {
            if (booking.StartTime.Date == booking.EndTime.Date) { 
                return booking.StartTime.TimeOfDay >= _forbiddenPeriodEnd.ToTimeSpan() &&
                    booking.EndTime.TimeOfDay <= _forbiddenPeriodStart.ToTimeSpan();
            } 
            else return false; 
        }
    }
}