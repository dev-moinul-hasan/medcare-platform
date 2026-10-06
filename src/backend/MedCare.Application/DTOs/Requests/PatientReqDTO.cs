using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Requests
{
    public sealed record class PatientReqDTO
    {
        [Required(ErrorMessage = "{0} is required")]
        [MaxLength(100, ErrorMessage = "{0} exceeds the limit of {1} characters.")]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "{0} exceeds the limit of {1} characters.")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [MaxLength(100, ErrorMessage = "{0} exceeds the limit of {1} characters.")]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? GenderId { get; set; }

        public string? MobileNo { get; set; }

        public string? NationalId { get; set; }

        public int PrimaryBranchId { get; set; }

        public int? BloodGroupId { get; set; }
    }
}
