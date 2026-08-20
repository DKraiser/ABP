using System.ComponentModel.DataAnnotations;

namespace ABP.Api.Options.InitialPolicies;

public class ForbiddenPeriodPolicyOptions
{
    public const string ConfigurationSectionName = "ForbiddenPeriodPolicies";

    [Required]
    [Range(0, 23)]
    public int StartHour { get; set; }

    [Range(0, 59)]
    public int StartMinute { get; set; } = 0;

    [Required]
    [Range(0, 23)]
    public int EndHour { get; set; }

    [Range(0, 59)]
    public int EndMinute { get; set; } = 0;
}