namespace MedCare.Application.DTOs.Responses.Core;

public sealed record NationalityRespDTO
{
    public int NationalityId { get; set; }
    public required string NationalityName { get; set; }
    public bool IsAvailable { get; set; }
}
