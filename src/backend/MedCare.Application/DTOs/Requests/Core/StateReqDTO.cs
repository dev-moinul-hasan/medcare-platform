using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Requests.Core;

public sealed record StateReqDTO
{
    [Required]
    [MaxLength(100)]
    public string? StateName { get; set; }

    [Required]
    public int CountryId { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}
