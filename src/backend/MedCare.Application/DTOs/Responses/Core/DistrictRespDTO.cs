namespace MedCare.Application.DTOs.Responses.Core;

public sealed record DistrictRespDTO
{
    public int DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public int StateId { get; set; }
    public bool IsActive{ get; set; }
}
