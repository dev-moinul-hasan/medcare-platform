namespace MedCare.Application.DTOs.Responses.Core;

public sealed record StateRespDTO
{
    public int StateId { get; set; }
    public string? StateName { get; set; }
    public int CountryId { get; set; }
    public bool IsActive { get; set; }
}
