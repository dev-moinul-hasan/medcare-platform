using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Requests.Core;

public sealed record CityReqDTO
{
    [Required]
    [MaxLength(100)]
    public string? CityName { get; set; }

    [Required]
    public int StateId { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}
