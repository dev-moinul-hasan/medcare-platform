-- ============================================================
-- HMS FINAL ENTERPRISE SCHEMA (v3 — supersedes the trimmed v1 pass)
-- ============================================================

IF SCHEMA_ID('core') IS NULL EXEC sp_executesql N'CREATE SCHEMA [core]';
GO
IF SCHEMA_ID('party') IS NULL EXEC sp_executesql N'CREATE SCHEMA [party]';
GO
IF SCHEMA_ID('clinical') IS NULL EXEC sp_executesql N'CREATE SCHEMA [clinical]';
GO
IF SCHEMA_ID('billing') IS NULL EXEC sp_executesql N'CREATE SCHEMA [billing]';
GO
IF SCHEMA_ID('inventory') IS NULL EXEC sp_executesql N'CREATE SCHEMA [inventory]';
GO
IF SCHEMA_ID('diagnostic') IS NULL EXEC sp_executesql N'CREATE SCHEMA [diagnostic]';
GO
IF SCHEMA_ID('finance') IS NULL EXEC sp_executesql N'CREATE SCHEMA [finance]';
GO
IF SCHEMA_ID('security') IS NULL EXEC sp_executesql N'CREATE SCHEMA [security]';
GO
IF SCHEMA_ID('audit') IS NULL EXEC sp_executesql N'CREATE SCHEMA [audit]';
GO

 ============================================================
 CORE — org structure, facility hierarchy, typed lookups
 ============================================================

CREATE TABLE core.Company (
    CompanyId       INT IDENTITY PRIMARY KEY,
    ParentCompanyId INT NULL REFERENCES core.Company(CompanyId),  -- legacy maincompmast hierarchy
    CompanyName     NVARCHAR(200) NOT NULL,
    RegistrationNo  NVARCHAR(50),
    GSTIN           NVARCHAR(15),
    PAN             NVARCHAR(10),
    BankAccountName NVARCHAR(100),
    BankAccountNo   NVARCHAR(30),
    BankName        NVARCHAR(100),
    BankIFSC        NVARCHAR(15),
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedByUserId INT NULL,
    CreatedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedByUserId INT NULL,
    ModifiedAtUtc   DATETIME2 NULL
);

CREATE TABLE core.Branch (
    BranchId    INT IDENTITY PRIMARY KEY,
    CompanyId   INT NOT NULL REFERENCES core.Company(CompanyId),
    BranchName  NVARCHAR(200) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1
);

CREATE TABLE core.Building (
    BuildingId  INT IDENTITY PRIMARY KEY,
    BranchId    INT NOT NULL REFERENCES core.Branch(BranchId),
    BuildingName NVARCHAR(100) NOT NULL
);

CREATE TABLE core.Floor (
    FloorId     INT IDENTITY PRIMARY KEY,
    BuildingId  INT NOT NULL REFERENCES core.Building(BuildingId),
    FloorName   NVARCHAR(50) NOT NULL
);

CREATE TABLE core.Department (
    DepartmentId    INT IDENTITY PRIMARY KEY,
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    DepartmentName  NVARCHAR(100) NOT NULL,
    DepartmentType  NVARCHAR(30) NOT NULL   -- Clinical / Diagnostic / Pharmacy / Admin
);

CREATE TABLE core.Room (
    RoomId          INT IDENTITY PRIMARY KEY,
    FloorId         INT NOT NULL REFERENCES core.Floor(FloorId),
    DepartmentId    INT NULL REFERENCES core.Department(DepartmentId),
    RoomNo          NVARCHAR(20) NOT NULL,
    RoomType        NVARCHAR(30) NOT NULL   -- General / Semi-Private / Private / ICU / OT
);

CREATE TABLE core.Bed (
    BedId       INT IDENTITY PRIMARY KEY,
    RoomId      INT NOT NULL REFERENCES core.Room(RoomId),
    BedNo       NVARCHAR(20) NOT NULL,
    BedType     NVARCHAR(30) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1
);

 Geography: a real hierarchy, not a generic lookup
CREATE TABLE core.Country (
    CountryId   INT IDENTITY PRIMARY KEY,
    CountryName NVARCHAR(100) NOT NULL,
    ISOCode     CHAR(2)
);

CREATE TABLE core.State (
    StateId     INT IDENTITY PRIMARY KEY,
    CountryId   INT NOT NULL REFERENCES core.Country(CountryId),
    StateName   NVARCHAR(100) NOT NULL
);

CREATE TABLE core.City (
    CityId      INT IDENTITY PRIMARY KEY,
    StateId     INT NOT NULL REFERENCES core.State(StateId),
    CityName    NVARCHAR(100) NOT NULL
);

 Small, purpose-grouped typed lookups (replaces one giant ProjHelp)
CREATE TABLE core.Lookup_Demographic (
    -- Gender, Religion, MaritalStatus, BloodGroup, Nationality, Occupation, Relation
    LookupId    INT IDENTITY PRIMARY KEY,
    Category    NVARCHAR(30) NOT NULL,
    Code        NVARCHAR(20) NOT NULL,
    Label       NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_LookupDemo UNIQUE (Category, Code)
);

CREATE TABLE core.Lookup_ClinicalStatus (
    -- EncounterStatus, AppointmentStatus, DiagnosisType, AllocationStatus, DischargeType
    LookupId    INT IDENTITY PRIMARY KEY,
    Category    NVARCHAR(30) NOT NULL,
    Code        NVARCHAR(20) NOT NULL,
    Label       NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_LookupClinStatus UNIQUE (Category, Code)
);

CREATE TABLE core.Lookup_Billing (
    -- InvoiceStatus, ChargeStatus, PaymentMethod, ServiceCategory, ConcessionScheme
    LookupId    INT IDENTITY PRIMARY KEY,
    Category    NVARCHAR(30) NOT NULL,
    Code        NVARCHAR(20) NOT NULL,
    Label       NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_LookupBilling UNIQUE (Category, Code)
);

CREATE TABLE core.Lookup_Inventory (
    -- IndentStatus, DispenseStatus
    LookupId    INT IDENTITY PRIMARY KEY,
    Category    NVARCHAR(30) NOT NULL,
    Code        NVARCHAR(20) NOT NULL,
    Label       NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_LookupInventory UNIQUE (Category, Code)
);

CREATE TABLE core.Lookup_Misc (
    -- Small ungrouped code/description masters with no clear domain home
    -- (e.g. legacy provdmast) -- explicit, not a catch-all dumping ground
    LookupId    INT IDENTITY PRIMARY KEY,
    Category    NVARCHAR(30) NOT NULL,
    Code        NVARCHAR(20) NOT NULL,
    Label       NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_LookupMisc UNIQUE (Category, Code)
);

 ADDED (gap fill): the service-line master almost everything else references
CREATE TABLE core.Service (
    ServiceId   INT IDENTITY PRIMARY KEY,
    BranchId    INT NOT NULL REFERENCES core.Branch(BranchId),
    ServiceName NVARCHAR(100) NOT NULL,
    ServiceType NVARCHAR(30) NOT NULL,   -- Clinical / Diagnostic / Support
    IsActive    BIT NOT NULL DEFAULT 1
);

 ADDED (gap fill): regulatory/registration tracking with expiry reminders
CREATE TABLE core.License (
    LicenseId       INT IDENTITY PRIMARY KEY,
    EntityType      NVARCHAR(30) NOT NULL,   -- Branch / Provider / Staff / Department
    EntityId        INT NOT NULL,
    LicenseName     NVARCHAR(100) NOT NULL,
    IssuingAuthority NVARCHAR(100),
    RegisteredDate  DATE,
    ExpiryDate      DATE,
    ReminderDate    DATE,
    IsValid         BIT NOT NULL DEFAULT 1,
    Remarks         NVARCHAR(300)
);

 ============================================================
 PARTY — shared identity model
 ============================================================

CREATE TABLE party.Party (
    PartyId     INT IDENTITY PRIMARY KEY,
    PartyType   NVARCHAR(20) NOT NULL   -- Person / Organization
);

CREATE TABLE party.Person (
    PartyId     INT PRIMARY KEY REFERENCES party.Party(PartyId),
    FirstName   NVARCHAR(100) NOT NULL,
    LastName    NVARCHAR(100),
    DateOfBirth DATE,
    GenderLookupId INT REFERENCES core.Lookup_Demographic(LookupId),
    MobileNo    NVARCHAR(20),
    NationalId  NVARCHAR(30)
);
CREATE INDEX IX_Person_MobileNo ON party.Person(MobileNo);

CREATE TABLE party.Organization (
    PartyId             INT PRIMARY KEY REFERENCES party.Party(PartyId),
    OrganizationName    NVARCHAR(200) NOT NULL,
    OrganizationType    NVARCHAR(30) NOT NULL,  -- Payer/TPA / Vendor / ReferralSource
    GSTIN               NVARCHAR(15),
    PAN                 NVARCHAR(10),
    AccountId           INT NULL   -- FK to finance.Account, added after finance schema below
);

CREATE TABLE party.Patient (
    PatientId       INT IDENTITY PRIMARY KEY,
    PartyId         INT NOT NULL UNIQUE REFERENCES party.Person(PartyId),
    PatientNo       NVARCHAR(30) NOT NULL UNIQUE,   -- UHID
    PrimaryBranchId INT REFERENCES core.Branch(BranchId),
    BloodGroupLookupId INT REFERENCES core.Lookup_Demographic(LookupId)
);

CREATE TABLE party.Provider (
    ProviderId      INT IDENTITY PRIMARY KEY,
    PartyId         INT NOT NULL UNIQUE REFERENCES party.Person(PartyId),
    ProviderType    NVARCHAR(30) NOT NULL,  -- Doctor / Consultant / Therapist
    SpecialtyLookupId INT REFERENCES core.Lookup_Demographic(LookupId)
);

CREATE TABLE party.Staff (
    StaffId         INT IDENTITY PRIMARY KEY,
    PartyId         INT NOT NULL UNIQUE REFERENCES party.Person(PartyId),
    StaffCode       NVARCHAR(30) NOT NULL UNIQUE,
    PrimaryBranchId INT REFERENCES core.Branch(BranchId),
    PrimaryDepartmentId INT REFERENCES core.Department(DepartmentId),
    EmploymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Active'
);

 ADDED (gap fill): referral/agent commission tracking (legacy refmast + salesmanmast)
CREATE TABLE party.CommissionAgreement (
    CommissionAgreementId INT IDENTITY PRIMARY KEY,
    PartyId         INT NOT NULL REFERENCES party.Party(PartyId),  -- referring doctor, org, or sales agent
    ComRate         DECIMAL(5,2) NOT NULL DEFAULT 0,
    OPDComRate      DECIMAL(5,2) NOT NULL DEFAULT 0,
    IPDComRate      DECIMAL(5,2) NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1,
    EffectiveFrom   DATE NOT NULL,
    EffectiveTo     DATE NULL
);

 ============================================================
 CLINICAL
 ============================================================

CREATE TABLE clinical.Encounter (
    EncounterId     INT IDENTITY PRIMARY KEY,
    EncounterNo     NVARCHAR(30) NOT NULL UNIQUE,
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    DepartmentId    INT REFERENCES core.Department(DepartmentId),
    ProviderId      INT REFERENCES party.Provider(ProviderId),
    EncounterType   NVARCHAR(20) NOT NULL,   -- OPD / IPD / ER / Daycare
    EncounterDateTime DATETIME2 NOT NULL,
    StatusLookupId  INT REFERENCES core.Lookup_ClinicalStatus(LookupId),
    SourceEncounterId INT NULL REFERENCES clinical.Encounter(EncounterId),
    ReferringPartyId INT NULL REFERENCES party.Party(PartyId),
    CreatedByUserId INT NULL,
    CreatedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedByUserId INT NULL,
    ModifiedAtUtc   DATETIME2 NULL
);
CREATE INDEX IX_Encounter_Patient_Date ON clinical.Encounter(PatientId, EncounterDateTime);

CREATE TABLE clinical.Appointment (
    AppointmentId   INT IDENTITY PRIMARY KEY,
    AppointmentNo   NVARCHAR(30) NOT NULL UNIQUE,
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    ProviderId      INT NOT NULL REFERENCES party.Provider(ProviderId),
    EncounterId     INT NULL REFERENCES clinical.Encounter(EncounterId),
    ScheduledDateTime DATETIME2 NOT NULL,
    StatusLookupId  INT REFERENCES core.Lookup_ClinicalStatus(LookupId)
);

CREATE TABLE clinical.ProviderSchedule (
    -- GAP FILL: collapses legacy docshdlmast/shdlmast/shiftmast/daymast/consabsent
    ProviderScheduleId INT IDENTITY PRIMARY KEY,
    ProviderId      INT NOT NULL REFERENCES party.Provider(ProviderId),
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    DayOfWeek       TINYINT NOT NULL,       -- 0=Sun .. 6=Sat
    StartTime       TIME NOT NULL,
    EndTime         TIME NOT NULL,
    IsAvailable     BIT NOT NULL DEFAULT 1, -- covers "absent" as a false-flag row instead of a separate table
    EffectiveFrom   DATE NOT NULL,
    EffectiveTo     DATE NULL
);

CREATE TABLE clinical.BedAllocation (
    BedAllocationId INT IDENTITY PRIMARY KEY,
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    BedId           INT NOT NULL REFERENCES core.Bed(BedId),
    FromDateTimeUtc DATETIME2 NOT NULL,
    ToDateTimeUtc   DATETIME2 NULL,
    StatusLookupId  INT REFERENCES core.Lookup_ClinicalStatus(LookupId)
);
CREATE INDEX IX_BedAlloc_Encounter_History ON clinical.BedAllocation(EncounterId, FromDateTimeUtc);

CREATE TABLE clinical.VitalSign (
    VitalSignId     INT IDENTITY PRIMARY KEY,
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    RecordedByStaffId INT REFERENCES party.Staff(StaffId),
    RecordedDateTimeUtc DATETIME2 NOT NULL,
    Temperature     DECIMAL(5,2),
    PulseRate       SMALLINT,
    RespirationRate SMALLINT,
    BpSystolic      SMALLINT,
    BpDiastolic     SMALLINT,
    SpO2            SMALLINT
);

CREATE TABLE clinical.ClinicalNote (
    ClinicalNoteId  INT IDENTITY PRIMARY KEY,
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    NoteType        NVARCHAR(30) NOT NULL,  -- ChiefComplaint / Progress / History / Discharge
    NoteText        NVARCHAR(MAX) NOT NULL,
    RecordedByProviderId INT REFERENCES party.Provider(ProviderId),
    NoteDateTimeUtc DATETIME2 NOT NULL,
    AmendedNoteId   INT NULL REFERENCES clinical.ClinicalNote(ClinicalNoteId)
);
CREATE INDEX IX_ClinicalNote_Patient_Timeline ON clinical.ClinicalNote(PatientId, NoteDateTimeUtc);

CREATE TABLE clinical.PatientHistory (
    -- consolidates legacy sysdiseasesmast/detl, pasthistorymast, pastmedhistorymast, allergymast/detl
    PatientHistoryId INT IDENTITY PRIMARY KEY,
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    HistoryType     NVARCHAR(30) NOT NULL,  -- SystemicDisease / PastSurgical / PastMedical / Allergy
    Description     NVARCHAR(300) NOT NULL,
    Severity        NVARCHAR(20) NULL,      -- used for Allergy rows
    RecordedDateTimeUtc DATETIME2 NOT NULL
);

CREATE TABLE clinical.DiagnosisCode (
    DiagnosisCodeId INT IDENTITY PRIMARY KEY,
    Code            NVARCHAR(20) NOT NULL,
    CodingSystem    NVARCHAR(20) NOT NULL,  -- ICD-10 etc.
    Description     NVARCHAR(300) NOT NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_DiagnosisCode_Code_System UNIQUE (Code, CodingSystem)
);

CREATE TABLE clinical.EncounterDiagnosis (
    EncounterDiagnosisId INT IDENTITY PRIMARY KEY,
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    DiagnosisCodeId INT NOT NULL REFERENCES clinical.DiagnosisCode(DiagnosisCodeId),
    DiagnosedByProviderId INT REFERENCES party.Provider(ProviderId),
    DiagnosisTypeLookupId INT REFERENCES core.Lookup_ClinicalStatus(LookupId)
);

CREATE TABLE clinical.PrescriptionOrder (
    PrescriptionOrderId INT IDENTITY PRIMARY KEY,
    PrescriptionNo  NVARCHAR(30) NOT NULL UNIQUE,
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    ProviderId      INT NOT NULL REFERENCES party.Provider(ProviderId),
    OrderDateTimeUtc DATETIME2 NOT NULL,
    StatusLookupId  INT REFERENCES core.Lookup_ClinicalStatus(LookupId)
);

CREATE TABLE clinical.PrescriptionDetail (
    PrescriptionDetailId INT IDENTITY PRIMARY KEY,
    PrescriptionOrderId INT NOT NULL REFERENCES clinical.PrescriptionOrder(PrescriptionOrderId),
    ItemId          INT NOT NULL,  -- FK to inventory.ItemMaster
    Dosage          NVARCHAR(50),
    Frequency       NVARCHAR(50),
    Route           NVARCHAR(30),
    DurationDays    SMALLINT,
    StatusLookupId  INT REFERENCES core.Lookup_ClinicalStatus(LookupId)
);
ALTER TABLE clinical.PrescriptionDetail
    ADD CONSTRAINT FK_PrescDetail_Item FOREIGN KEY (ItemId) REFERENCES inventory.ItemMaster(ItemId);

CREATE TABLE clinical.DischargeSummary (
    DischargeSummaryId INT IDENTITY PRIMARY KEY,
    EncounterId     INT NOT NULL UNIQUE REFERENCES clinical.Encounter(EncounterId),
    DischargeTypeLookupId INT REFERENCES core.Lookup_ClinicalStatus(LookupId),
    DischargedByProviderId INT REFERENCES party.Provider(ProviderId),
    DischargeDateTimeUtc DATETIME2 NOT NULL,
    SummaryText     NVARCHAR(MAX)
);

CREATE TABLE clinical.SurgeryNote (
    -- GAP FILL: minimal replacement for legacy surgmast/surgdetl/surgappmast/surgappdetl/surgrec/anaplanmast
    SurgeryNoteId   INT IDENTITY PRIMARY KEY,
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    SurgeonProviderId INT NOT NULL REFERENCES party.Provider(ProviderId),
    ProcedureName   NVARCHAR(200) NOT NULL,
    ScheduledDateTimeUtc DATETIME2 NOT NULL,
    AnaesthesiaPlan NVARCHAR(300),
    StatusLookupId  INT REFERENCES core.Lookup_ClinicalStatus(LookupId),
    NotesText       NVARCHAR(MAX)
);

 ADDED (gap fill): medico-legal case tracking, missing from both prior passes
CREATE TABLE clinical.MLCRecord (
    MLCRecordId     INT IDENTITY PRIMARY KEY,
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    MLCTypeLookupId INT REFERENCES core.Lookup_ClinicalStatus(LookupId),  -- category 'MLCType'
    PoliceStationName NVARCHAR(150),
    FIRNo           NVARCHAR(50),
    InformedDateTimeUtc DATETIME2,
    InformedByStaffId INT REFERENCES party.Staff(StaffId),
    Remarks         NVARCHAR(500)
);

 ============================================================
 BILLING (finance/GL module intentionally removed)
 ============================================================

CREATE TABLE billing.TariffRate (
    TariffRateId    INT IDENTITY PRIMARY KEY,
    ServiceCategoryLookupId INT REFERENCES core.Lookup_Billing(LookupId),
    ServiceEntityId INT NOT NULL,      -- polymorphic ref: TestId / ItemId / ProcedureId
    RoomTypeId      INT NULL REFERENCES core.Room(RoomId),
    Rate            DECIMAL(12,2) NOT NULL,
    EffectiveFromDate DATE NOT NULL,
    CONSTRAINT UQ_TariffRate_Lookup UNIQUE (ServiceCategoryLookupId, ServiceEntityId, RoomTypeId, EffectiveFromDate)
);

CREATE TABLE billing.Charge (
    ChargeId        INT IDENTITY PRIMARY KEY,
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    ChargeSourceLookupId INT REFERENCES core.Lookup_Billing(LookupId),  -- OPD/IPD/Pharmacy/Lab/etc.
    Amount          DECIMAL(12,2) NOT NULL,
    ChargeStatusLookupId INT REFERENCES core.Lookup_Billing(LookupId),
    ChargeDateTimeUtc DATETIME2 NOT NULL,
    CreatedByUserId INT NULL,
    CreatedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedByUserId INT NULL,
    ModifiedAtUtc   DATETIME2 NULL
);

CREATE TABLE billing.Invoice (
    InvoiceId       INT IDENTITY PRIMARY KEY,
    InvoiceNo       NVARCHAR(30) NOT NULL UNIQUE,
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    PayerPartyId    INT NULL REFERENCES party.Party(PartyId),   -- self-pay if null; else Org (TPA/Insurer)
    TotalAmount     DECIMAL(12,2) NOT NULL,
    InvoiceStatusLookupId INT REFERENCES core.Lookup_Billing(LookupId),
    InvoiceDateTimeUtc DATETIME2 NOT NULL,
    CreatedByUserId INT NULL,
    CreatedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedByUserId INT NULL,
    ModifiedAtUtc   DATETIME2 NULL
);

CREATE TABLE billing.PaymentReceipt (
    PaymentReceiptId INT IDENTITY PRIMARY KEY,
    ReceiptNo       NVARCHAR(30) NOT NULL UNIQUE,
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    EncounterId     INT NULL REFERENCES clinical.Encounter(EncounterId),
    PayerPartyId    INT NULL REFERENCES party.Party(PartyId),
    Amount          DECIMAL(12,2) NOT NULL,
    PaymentMethodLookupId INT REFERENCES core.Lookup_Billing(LookupId),
    ReceiptDateTimeUtc DATETIME2 NOT NULL,
    CreatedByUserId INT NULL,
    CreatedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedByUserId INT NULL,
    ModifiedAtUtc   DATETIME2 NULL
);

CREATE TABLE billing.PaymentAllocation (
    PaymentAllocationId INT IDENTITY PRIMARY KEY,
    PaymentReceiptId INT NOT NULL REFERENCES billing.PaymentReceipt(PaymentReceiptId),
    InvoiceId       INT NOT NULL REFERENCES billing.Invoice(InvoiceId),
    AllocatedAmount DECIMAL(12,2) NOT NULL
);

 ============================================================
 INVENTORY (de-scoped: single-location stock balance, no batch/transfer)
 ============================================================

CREATE TABLE inventory.ItemCategory (
    ItemCategoryId  INT IDENTITY PRIMARY KEY,
    CategoryName    NVARCHAR(100) NOT NULL
);

CREATE TABLE inventory.ItemMaster (
    ItemId          INT IDENTITY PRIMARY KEY,
    ItemName        NVARCHAR(200) NOT NULL,
    ItemCategoryId  INT REFERENCES inventory.ItemCategory(ItemCategoryId),
    ItemType        NVARCHAR(20) NOT NULL,  -- Drug / Consumable / Implant
    UnitOfMeasure   NVARCHAR(20) NOT NULL
);

CREATE TABLE inventory.StockBalance (
    StockBalanceId  INT IDENTITY PRIMARY KEY,
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    ItemId          INT NOT NULL REFERENCES inventory.ItemMaster(ItemId),
    QuantityOnHand  DECIMAL(12,2) NOT NULL DEFAULT 0,
    LastUpdatedUtc  DATETIME2 NOT NULL,
    CONSTRAINT UQ_StockBalance_Branch_Item UNIQUE (BranchId, ItemId)
);

CREATE TABLE inventory.StockDispense (
    StockDispenseId INT IDENTITY PRIMARY KEY,
    DispenseNo      NVARCHAR(30) NOT NULL UNIQUE,
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    PrescriptionOrderId INT REFERENCES clinical.PrescriptionOrder(PrescriptionOrderId),
    ChargeId        INT REFERENCES billing.Charge(ChargeId),
    DispensedByStaffId INT REFERENCES party.Staff(StaffId),
    DispenseDateTimeUtc DATETIME2 NOT NULL
);

CREATE TABLE inventory.StockDispenseDetail (
    StockDispenseDetailId INT IDENTITY PRIMARY KEY,
    StockDispenseId INT NOT NULL REFERENCES inventory.StockDispense(StockDispenseId),
    ItemId          INT NOT NULL REFERENCES inventory.ItemMaster(ItemId),
    Quantity        DECIMAL(10,2) NOT NULL
);

 ADDED (gap fill): branch-scoped stock point (legacy stkpmast) and
 ward-to-pharmacy indent workflow (legacy inddetlipd/indmastipd), kept
 minimal -- not full multi-warehouse batch tracking
CREATE TABLE inventory.StockPoint (
    StockPointId    INT IDENTITY PRIMARY KEY,
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    StockPointCode  NVARCHAR(20) NOT NULL,
    StockPointName  NVARCHAR(100) NOT NULL,
    Location        NVARCHAR(100),
    CONSTRAINT UQ_StockPoint_Branch_Code UNIQUE (BranchId, StockPointCode)
);

CREATE TABLE inventory.StockIndent (
    StockIndentId   INT IDENTITY PRIMARY KEY,
    IndentNo        NVARCHAR(30) NOT NULL UNIQUE,
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    RequestingDepartmentId INT NOT NULL REFERENCES core.Department(DepartmentId),
    StockPointId    INT NOT NULL REFERENCES inventory.StockPoint(StockPointId),
    RequestedByStaffId INT REFERENCES party.Staff(StaffId),
    IndentDateTimeUtc DATETIME2 NOT NULL,
    StatusLookupId  INT REFERENCES core.Lookup_Inventory(LookupId)  -- category 'IndentStatus'
);

CREATE TABLE inventory.StockIndentDetail (
    StockIndentDetailId INT IDENTITY PRIMARY KEY,
    StockIndentId   INT NOT NULL REFERENCES inventory.StockIndent(StockIndentId),
    ItemId          INT NOT NULL REFERENCES inventory.ItemMaster(ItemId),
    QuantityRequested DECIMAL(10,2) NOT NULL,
    QuantityIssued  DECIMAL(10,2) NOT NULL DEFAULT 0
);

 ============================================================
 DIAGNOSTIC (de-scoped: no sample chain-of-custody, no DICOM fields)
 ============================================================

 ADDED (gap fill): test-profile grouping (legacy profmast), tied to core.Service
CREATE TABLE diagnostic.TestProfile (
    TestProfileId   INT IDENTITY PRIMARY KEY,
    ServiceId       INT NOT NULL REFERENCES core.Service(ServiceId),
    ProfileCode     NVARCHAR(20) NOT NULL UNIQUE,
    ProfileName     NVARCHAR(150) NOT NULL,
    IsActive        BIT NOT NULL DEFAULT 1
);

-- ADDED (gap fill): body-part master with rate (legacy partmast)
CREATE TABLE diagnostic.BodyPart (
    BodyPartId      INT IDENTITY PRIMARY KEY,
    BodyPartCode    NVARCHAR(20) NOT NULL UNIQUE,
    BodyPartName    NVARCHAR(100) NOT NULL,
    Rate            DECIMAL(12,2)
);

CREATE TABLE diagnostic.DiagnosticTest (
    TestId          INT IDENTITY PRIMARY KEY,
    TestCode        NVARCHAR(20) NOT NULL UNIQUE,
    TestName        NVARCHAR(200) NOT NULL,
    TestCategory    NVARCHAR(30) NOT NULL,  -- Lab / Imaging
    TestProfileId   INT NULL REFERENCES diagnostic.TestProfile(TestProfileId),
    BodyPartId      INT NULL REFERENCES diagnostic.BodyPart(BodyPartId)
);

CREATE TABLE diagnostic.TestPackage (
    TestPackageId   INT IDENTITY PRIMARY KEY,
    PackageName     NVARCHAR(200) NOT NULL
);

CREATE TABLE diagnostic.TestPackageItem (
    TestPackageItemId INT IDENTITY PRIMARY KEY,
    TestPackageId   INT NOT NULL REFERENCES diagnostic.TestPackage(TestPackageId),
    TestId          INT NOT NULL REFERENCES diagnostic.DiagnosticTest(TestId)
);

CREATE TABLE diagnostic.DiagnosticOrder (
    DiagnosticOrderId INT IDENTITY PRIMARY KEY,
    OrderNo         NVARCHAR(30) NOT NULL UNIQUE,
    PatientId       INT NOT NULL REFERENCES party.Patient(PatientId),
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    EncounterId     INT NOT NULL REFERENCES clinical.Encounter(EncounterId),
    OrderingProviderId INT REFERENCES party.Provider(ProviderId),
    OrderDateTimeUtc DATETIME2 NOT NULL,
    StatusLookupId  INT REFERENCES core.Lookup_ClinicalStatus(LookupId)
);

CREATE TABLE diagnostic.DiagnosticOrderDetail (
    DiagnosticOrderDetailId INT IDENTITY PRIMARY KEY,
    DiagnosticOrderId INT NOT NULL REFERENCES diagnostic.DiagnosticOrder(DiagnosticOrderId),
    TestId          INT NOT NULL REFERENCES diagnostic.DiagnosticTest(TestId),
    StatusLookupId  INT REFERENCES core.Lookup_ClinicalStatus(LookupId)  -- includes Rejected as a status
);

CREATE TABLE diagnostic.TestResult (
    TestResultId    INT IDENTITY PRIMARY KEY,
    DiagnosticOrderDetailId INT NOT NULL UNIQUE REFERENCES diagnostic.DiagnosticOrderDetail(DiagnosticOrderDetailId),
    ResultText      NVARCHAR(MAX),
    ResultStatusLookupId INT REFERENCES core.Lookup_ClinicalStatus(LookupId),
    EnteredByStaffId INT REFERENCES party.Staff(StaffId),
    VerifiedByStaffId INT REFERENCES party.Staff(StaffId),
    ResultDateTimeUtc DATETIME2 NOT NULL
);

CREATE TABLE diagnostic.ImagingStudy (
    -- trimmed: no DICOM-specific fields (Modality/Laterality/StudyInstanceUid) unless in-house PACS exists
    ImagingStudyId  INT IDENTITY PRIMARY KEY,
    DiagnosticOrderDetailId INT NOT NULL REFERENCES diagnostic.DiagnosticOrderDetail(DiagnosticOrderDetailId),
    PerformedByStaffId INT REFERENCES party.Staff(StaffId),
    ReportedByProviderId INT REFERENCES party.Provider(ProviderId),
    ReportText      NVARCHAR(MAX),
    StudyDateTimeUtc DATETIME2 NOT NULL
);

 --============================================================
 --FINANCE (reinstated, lean: chart of accounts + journal posting +
 --vendor/customer sub-ledger via party.Organization.AccountId.
 --============================================================

CREATE TABLE finance.FiscalPeriod (
    FiscalPeriodId  INT IDENTITY PRIMARY KEY,
    FiscalYear      NVARCHAR(9) NOT NULL,      
    PeriodNo        TINYINT NOT NULL,          -- 1-12
    StartDate       DATE NOT NULL,
    EndDate         DATE NOT NULL,
    IsClosed        BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_FiscalPeriod UNIQUE (FiscalYear, PeriodNo)
);

CREATE TABLE finance.Account (
    AccountId       INT IDENTITY PRIMARY KEY,
    CompanyId       INT NOT NULL REFERENCES core.Company(CompanyId),
    AccountCode     NVARCHAR(20) NOT NULL,
    AccountName     NVARCHAR(150) NOT NULL,
    AccountGroup    NVARCHAR(50) NOT NULL,     -- Asset / Liability / Income / Expense / Equity
    AccountCategory NVARCHAR(50),
    ParentAccountId INT NULL REFERENCES finance.Account(AccountId),
    IsControlAccount BIT NOT NULL DEFAULT 0,   -- true for vendor/customer control accounts
    IsActive        BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Account_Company_Code UNIQUE (CompanyId, AccountCode)
);

ALTER TABLE party.Organization
    ADD CONSTRAINT FK_Organization_Account FOREIGN KEY (AccountId) REFERENCES finance.Account(AccountId);

CREATE TABLE finance.JournalEntry (
    JournalEntryId  INT IDENTITY PRIMARY KEY,
    JournalEntryNo  NVARCHAR(30) NOT NULL UNIQUE,
    CompanyId       INT NOT NULL REFERENCES core.Company(CompanyId),
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    FiscalPeriodId  INT NOT NULL REFERENCES finance.FiscalPeriod(FiscalPeriodId),
    DocType         NVARCHAR(30) NOT NULL,     -- Invoice / Receipt / Purchase / Journal / Adjustment
    DocDate         DATE NOT NULL,
    SourceReferenceType NVARCHAR(30),          -- e.g. 'billing.Invoice', 'billing.PaymentReceipt'
    SourceReferenceId INT,
    Narration       NVARCHAR(300),
    CreatedByUserId INT NULL,
    CreatedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedByUserId INT NULL,
    ModifiedAtUtc   DATETIME2 NULL
);

CREATE TABLE finance.JournalEntryLine (
    JournalEntryLineId INT IDENTITY PRIMARY KEY,
    JournalEntryId  INT NOT NULL REFERENCES finance.JournalEntry(JournalEntryId),
    AccountId       INT NOT NULL REFERENCES finance.Account(AccountId),
    PartyId         INT NULL REFERENCES party.Party(PartyId),  -- populated for control-account lines
    DebitAmount     DECIMAL(14,2) NOT NULL DEFAULT 0,
    CreditAmount    DECIMAL(14,2) NOT NULL DEFAULT 0,
    TaxableValue    DECIMAL(14,2) NOT NULL DEFAULT 0,
    CGSTRate        DECIMAL(5,2) NOT NULL DEFAULT 0,
    CGSTAmount      DECIMAL(14,2) NOT NULL DEFAULT 0,
    SGSTRate        DECIMAL(5,2) NOT NULL DEFAULT 0,
    SGSTAmount      DECIMAL(14,2) NOT NULL DEFAULT 0,
    IGSTRate        DECIMAL(5,2) NOT NULL DEFAULT 0,
    IGSTAmount      DECIMAL(14,2) NOT NULL DEFAULT 0,
    CONSTRAINT CK_JournalLine_OneSided CHECK (NOT (DebitAmount > 0 AND CreditAmount > 0))
);
CREATE INDEX IX_JournalEntryLine_Account ON finance.JournalEntryLine(AccountId);
CREATE INDEX IX_JournalEntryLine_Party ON finance.JournalEntryLine(PartyId);


 --============================================================
 --SECURITY (reduced: no session/token tables — those belong to the auth service)
 --============================================================

CREATE TABLE security.[User] (
    UserId          INT IDENTITY PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL UNIQUE,
    Email           NVARCHAR(100) NOT NULL UNIQUE,
    StaffId         INT NULL REFERENCES party.Staff(StaffId),
    IsActive        BIT NOT NULL DEFAULT 1
);

CREATE TABLE security.Role (
    RoleId          INT IDENTITY PRIMARY KEY,
    RoleName        NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE security.UserRole (
    UserId          INT NOT NULL REFERENCES security.[User](UserId),
    RoleId          INT NOT NULL REFERENCES security.Role(RoleId),
    PRIMARY KEY (UserId, RoleId)
);

CREATE TABLE security.UserBranch (
    UserBranchId    INT IDENTITY PRIMARY KEY,
    UserId          INT NOT NULL REFERENCES security.[User](UserId),
    BranchId        INT NOT NULL REFERENCES core.Branch(BranchId),
    CONSTRAINT UQ_UserBranch_User_Branch UNIQUE (UserId, BranchId)
);

 ============================================================
 AUDIT
 ============================================================

CREATE TABLE audit.AuditLog (
    AuditLogId      BIGINT IDENTITY PRIMARY KEY,
    UserId          INT NULL REFERENCES security.[User](UserId),
    BranchId        INT NULL REFERENCES core.Branch(BranchId),
    Action          NVARCHAR(50) NOT NULL,
    EntityName      NVARCHAR(100) NOT NULL,
    EntityId        NVARCHAR(50) NOT NULL,
    ActionDateTimeUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

