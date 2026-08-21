using ABP.Api.Options.InitialPolicies;
using ABP.Application.Implementations.Policies.Booking;
using ABP.Application.Implementations.Policies.Pricing.HoursPolicy;
using ABP.Application.Interfaces.Policies;

namespace ABP.Api.Extensions;

public static class PoliciesExtensions
{
    public static IServiceCollection AddForbiddenPeriodBookingPoliciesFromConfiguration(this IServiceCollection services, IConfigurationSection section) {
        var forbiddenPeriodPolicies = section.Get<IReadOnlyList<ForbiddenPeriodPolicyOptions>>();

        return services.AddBookingPolicies(
            forbiddenPeriodPolicies?.Select(p => new ForbiddenPeriodPolicy(
                new(p.StartHour, p.StartMinute),
                new(p.EndHour, p.EndMinute)
            )).ToList() ?? []
        );
    } 

    public static IServiceCollection AddHoursPricePoliciesFromConfiguration(this IServiceCollection services, IConfigurationSection section) {
        var hoursPricingPolicies = section.Get<IReadOnlyList<HoursPricePeriodPolicyOptions>>();
        
        return services.AddPricePolicies([
            new HoursPricePolicy(
                hoursPricingPolicies?.Select(p => new PricePeriod(
                    new (p.StartHour, p.StartMinute),
                    new (p.EndHour, p.EndMinute),
                    Convert.ToDecimal(p.Multiplier)
                )
            ).ToList() ?? [])
        ]);
    }

    public static IServiceCollection AddBookingPolicies(this IServiceCollection services, IReadOnlyList<IBookingPolicy> policies)
    {
        return services.AddSingleton<IReadOnlyList<IBookingPolicy>>(policies);
    }

    public static IServiceCollection AddPricePolicies(this IServiceCollection services, IReadOnlyList<IPricingPolicy> policies)
    {
        return services.AddSingleton<IReadOnlyList<IPricingPolicy>>(policies);
    }
}