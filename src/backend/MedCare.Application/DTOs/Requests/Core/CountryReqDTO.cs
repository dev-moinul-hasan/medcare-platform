using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Requests.Core;

public sealed record CountryReqDTO
{
    [Required]
    [MaxLength(100)]
    public string? CountryName { get; set; }

    public string? ISOCode { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}
