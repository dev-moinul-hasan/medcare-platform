namespace MedCare.Application.DTOs.Responses.party
{
    sealed record class PatientResp
    {
        public int PartyId { get; set; }

        public string? UHIDNo { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? GenderId { get; set; }

        public string? MobileNo { get; set; }

        public string? NationalId { get; set; }

        public int? BloodGroupId { get; set; }
    }
}
