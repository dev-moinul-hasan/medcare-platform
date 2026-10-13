using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Requests.Core;

public sealed record IDTypeReqDTO
{
    [Required]
    [MaxLength(100)]
    public string? IDTypeName { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}
