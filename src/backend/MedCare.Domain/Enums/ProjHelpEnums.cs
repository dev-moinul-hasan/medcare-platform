namespace MedCare.Domain.Enums;

public class ProjHelpEnums
{
    public enum ProjHelpCategory
    {
        Gender = 5,
        BloodGroup = 6,
        MaritalStatus = 10,
        ProviderType = 14,
        Specialty = 15,
        OrganizationType = 17,
        EncounterType = 18,
        EncounterStatus = 19,
        AppointmentStatus = 20,
        DiagnosisType = 21,
        BedAllocationStatus = 22,
        DischargeType = 23,
        ChargeStatus = 66,
        InvoiceStatus = 68,
        PaymentMethod = 69
    }

    public static class ProjHelpDefaults
    {
        // Encounter Status
        public const int EncounterActive = 1901;
        public const int EncounterDischarged = 1902;
        public const int EncounterCancelled = 1903;

        // Bed Status
        public const int BedOccupied = 2201;
        public const int BedVacated = 2202;

        // Billing Status
        public const int ChargePending = 6601;
        public const int ChargeBilled = 6602;
        public const int InvoiceDraft = 6801;
        public const int InvoicePaid = 6803;
    }
}
