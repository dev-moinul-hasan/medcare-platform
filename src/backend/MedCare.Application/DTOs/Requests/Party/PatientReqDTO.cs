using MedCare.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Requests.Party
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

        [Required (ErrorMessage = "{0} is required")]
        [EnumDataType(typeof(GenderEnum))]
        public GenderEnum GenderId { get; set; }

        public string? MobileNo { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? PinCode { get; set; }

        public int DistrinctId { get; set; }

        public int StateId { get; set; }

        public int CountryId { get; set; }

        public int RelagionId { get; set; }

        public int PrimaryBranchId { get; set; }

        [Required]
        [EnumDataType(typeof(BloodGroupEnum))]
        public BloodGroupEnum BloodGroupId { get; set; }

        [Required]
        [EnumDataType (typeof(OccupationEnums))]
        public OccupationEnums OccoupationId { get; set; }

        [Required]
        [EnumDataType(typeof(MaritalStatusEnum))]
        public MaritalStatusEnum MaritalStatus { get; set; }

        public double RegFees { get; set; } = 0;
    }
}
