namespace MedCare.Application.DTOs.Responses.Core;

public sealed record CityRespDTO
{
    public int CityId { get; set; }
    public string? CityName { get; set; }
    public int StateId { get; set; }
    public bool IsActive{ get; set; }
}
