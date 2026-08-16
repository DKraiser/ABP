namespace ABP.Application.Implementations.Policies.Pricing.HoursPolicy;

/// <summary>
/// Value object used pricing policy based on price periods.
/// </summary>
public class PricePeriod { 
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public decimal Multiplier { get; init; }

    public PricePeriod (TimeOnly startTime, TimeOnly endTime, decimal multiplier) { 
        if (startTime >= endTime) throw new ArgumentException("Price period duration must be a positive number.");
        if (multiplier <= 0) throw new ArgumentException("Price multiplier must be a positive number.");
        StartTime = startTime;
        EndTime = endTime;
        Multiplier = multiplier;
    }
}