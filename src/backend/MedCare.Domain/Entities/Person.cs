namespace MedCare.Domain.Entities;

sealed class Person
{
    public int PartyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int? GenderId { get; set; }
    public string? MobileNo { get; set; }
    public string? NationalId { get; set; }
}

