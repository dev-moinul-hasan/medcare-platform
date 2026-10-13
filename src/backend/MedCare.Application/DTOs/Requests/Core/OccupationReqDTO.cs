using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Requests.Core;

public sealed record OccupationReqDTO
{
    [Required]
    [MaxLength(100)]
    public string? OccupationName { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}
