namespace MedCare.Domain.Entities;

public sealed class ProjHelp
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int TypeCatgId { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}
