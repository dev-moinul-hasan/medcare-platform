namespace MedCare.Domain.Entities;

public sealed class Encounter
{
    public int EncounterId { get; set; }
    public string EncounterNo { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProviderId { get; set; }
    public int EncounterTypeId { get; set; }
    public DateTime EncounterDateTime { get; set; }
    public int StatusId { get; set; }
    public int? SourceEncounterId { get; set; }
    public int? ReferringPartyId { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int? ModifiedByUserId { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
