using System.ComponentModel.DataAnnotations;

namespace ABP.Api.Options.InitialRooms;

public sealed class ServiceOptions
{
    [Required]
    [MinLength(1)]
    public string? Name { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public decimal Price { get; set; }
}