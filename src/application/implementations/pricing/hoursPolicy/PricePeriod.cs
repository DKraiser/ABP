namespace ABP.Application.Implementations.Pricing;

/// <summary>
/// Value object used pricing policy based on price periods.
/// </summary>
public class PricePeriod { 
    TimeOnly StartTime { get; init; }
    TimeOnly EndTime { get; init; }
    float Multiplier { get; init; }

    public PricePeriod (TimeOnly startTime, TimeOnly endTime, float multiplier) { 
        if (startTime >= endTime) throw new ArgumentException("Price period duration must be a positive number.");
        if (multiplier <= 0) throw new ArgumentException("Price multiplier must be a positive number.");
        StartTime = startTime;
        EndTime = endTime;
        Multiplier = multiplier;
    }
}