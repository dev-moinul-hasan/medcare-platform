namespace MedCare.Application.DTOs.Responses.Core;

public sealed record OccupationRespDTO
{
    public int OccupationId { get; set; }
    public required string OccupationName { get; set; }
    public bool IsAvailable { get; set; }
}
