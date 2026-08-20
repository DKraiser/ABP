using ABP.Application.Interfaces.Policies;

namespace ABP.Api.Extensions;

public static class PoliciesExtensions
{
    public static IServiceCollection AddBookingPolicies(this IServiceCollection services, IReadOnlyList<IBookingPolicy> policies)
    {
        return services.AddSingleton<IReadOnlyList<IBookingPolicy>>(policies);
    }

    public static IServiceCollection AddPricingPolicies(this IServiceCollection services, IReadOnlyList<IPricingPolicy> policies)
    {
        return services.AddSingleton<IReadOnlyList<IPricingPolicy>>(policies);
    }
}