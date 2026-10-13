namespace MedCare.Application.DTOs.Responses.Core;

public sealed record ReligionRespDTO
{
    public int ReligionId { get; set; }
    public required string ReligionName { get; set; }
    public bool IsAvailable { get; set; }
}
