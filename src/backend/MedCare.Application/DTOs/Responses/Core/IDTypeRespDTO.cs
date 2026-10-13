namespace MedCare.Application.DTOs.Responses.Core;

public sealed record IDTypeRespDTO
{
    public int IDTypeId { get; set; }
    public required string IDTypeName { get; set; }
    public bool IsAvailable { get; set; }
}
