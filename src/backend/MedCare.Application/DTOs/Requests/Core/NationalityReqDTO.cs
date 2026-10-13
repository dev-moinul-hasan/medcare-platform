using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Requests.Core;

public sealed record NationalityReqDTO
{
    [Required]
    [MaxLength(100)]
    public string? NationalityName { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}
