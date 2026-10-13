using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Requests.Core;

public sealed record ReligionReqDTO
{
    [Required]
    [MaxLength(100)]
    public string? ReligionName { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}
