using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace ABP.Api.Options.InitialRooms;

public class RoomOptions
{
    public const string ConfigurationSectionName = "InitialRooms";

    [Required]
    [MinLength(1)]
    public string? Name { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Capacity { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public decimal BasePrice { get; set; }

    [ValidateEnumeratedItems]
    public IReadOnlyList<ServiceOptions>? AvailableServices { get; set; }
}