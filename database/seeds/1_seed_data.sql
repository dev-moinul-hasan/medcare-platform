-- ============================================================
-- SEED DATA — minimum rows required before this schema will accept
-- any transactional insert (Encounter, Invoice, JournalEntry, etc. all
-- carry NOT NULL/FK-constrained lookup, org, and period references).
-- Adjust values (company name, GSTIN, fiscal year) to your actual
-- deployment before running in a real environment.
-- ============================================================

-- ---------- ORG STRUCTURE ----------
INSERT INTO core.Company (CompanyName, RegistrationNo, GSTIN, PAN)
VALUES ('Sample Hospital Pvt Ltd', 'REG-0001', '19AAAAA0000A1Z5', 'AAAAA0000A');

INSERT INTO core.Branch (CompanyId, BranchName, BranchCode)
VALUES (1, 'Main Branch', 'MAIN');

-- ---------- GEOGRAPHY (minimum: one country/state/city; extend as needed) ----------
INSERT INTO core.Country (CountryName, ISOCode) VALUES ('India', 'IN');
INSERT INTO core.State (CountryId, StateName) VALUES (1, 'West Bengal');
INSERT INTO core.City (StateId, CityName) VALUES (1, 'Kolkata');

-- ---------- DEMOGRAPHIC LOOKUPS ----------
INSERT INTO core.Lookup_Demographic (Category, Code, Label) VALUES
('Gender', 'M', 'Male'),
('Gender', 'F', 'Female'),
('Gender', 'O', 'Other'),
('BloodGroup', 'A+', 'A Positive'),
('BloodGroup', 'A-', 'A Negative'),
('BloodGroup', 'B+', 'B Positive'),
('BloodGroup', 'B-', 'B Negative'),
('BloodGroup', 'AB+', 'AB Positive'),
('BloodGroup', 'AB-', 'AB Negative'),
('BloodGroup', 'O+', 'O Positive'),
('BloodGroup', 'O-', 'O Negative'),
('MaritalStatus', 'SGL', 'Single'),
('MaritalStatus', 'MRD', 'Married'),
('MaritalStatus', 'WDW', 'Widowed'),
('MaritalStatus', 'DVD', 'Divorced'),
('Religion', 'HIN', 'Hindu'),
('Religion', 'MUS', 'Muslim'),
('Religion', 'CHR', 'Christian'),
('Religion', 'SIK', 'Sikh'),
('Religion', 'OTH', 'Other'),
('Nationality', 'IND', 'Indian'),
('Nationality', 'OTH', 'Other'),
('Relation', 'FTH', 'Father'),
('Relation', 'MTH', 'Mother'),
('Relation', 'SPS', 'Spouse'),
('Relation', 'CHD', 'Child'),
('Relation', 'OTH', 'Other'),
('Occupation', 'SAL', 'Salaried'),
('Occupation', 'SEMP', 'Self-Employed'),
('Occupation', 'UNEMP', 'Unemployed'),
('Occupation', 'RETD', 'Retired'),
('SpecialtyOrProviderType', 'GEN', 'General Physician'),
('SpecialtyOrProviderType', 'SURG', 'Surgeon'),
('SpecialtyOrProviderType', 'PEDS', 'Pediatrician'),
('SpecialtyOrProviderType', 'GYN', 'Gynaecologist'),
('SpecialtyOrProviderType', 'ORTHO', 'Orthopaedist'),
('SpecialtyOrProviderType', 'CARD', 'Cardiologist'),
('SpecialtyOrProviderType', 'ANAES', 'Anaesthetist');

-- ---------- CLINICAL STATUS LOOKUPS ----------
INSERT INTO core.Lookup_ClinicalStatus (Category, Code, Label) VALUES
('EncounterStatus', 'ADM', 'Admitted / Active'),
('EncounterStatus', 'DIS', 'Discharged'),
('EncounterStatus', 'CAN', 'Cancelled'),
('AppointmentStatus', 'SCH', 'Scheduled'),
('AppointmentStatus', 'CNF', 'Confirmed'),
('AppointmentStatus', 'CMP', 'Completed'),
('AppointmentStatus', 'NSH', 'No-Show'),
('AppointmentStatus', 'CAN', 'Cancelled'),
('DiagnosisType', 'PRI', 'Primary'),
('DiagnosisType', 'SEC', 'Secondary'),
('DiagnosisType', 'PROV', 'Provisional'),
('AllocationStatus', 'OCC', 'Occupied'),
('AllocationStatus', 'VAC', 'Vacated'),
('DischargeType', 'NRM', 'Normal Discharge'),
('DischargeType', 'DAMA', 'Discharge Against Medical Advice'),
('DischargeType', 'REF', 'Referred Out'),
('DischargeType', 'EXP', 'Expired'),
('SurgeryStatus', 'SCH', 'Scheduled'),
('SurgeryStatus', 'CMP', 'Completed'),
('SurgeryStatus', 'CAN', 'Cancelled'),
('PrescriptionStatus', 'ACT', 'Active'),
('PrescriptionStatus', 'CMP', 'Completed'),
('PrescriptionStatus', 'CAN', 'Cancelled'),
('MLCType', 'ASLT', 'Assault'),
('MLCType', 'RTA', 'Road Traffic Accident'),
('MLCType', 'POIS', 'Poisoning'),
('MLCType', 'BURN', 'Burns'),
('MLCType', 'OTH', 'Other'),
('DiagnosticOrderStatus', 'ORD', 'Ordered'),
('DiagnosticOrderStatus', 'SMP', 'Sample/Study In Progress'),
('DiagnosticOrderStatus', 'RES', 'Resulted'),
('DiagnosticOrderStatus', 'REJ', 'Rejected'),
('DiagnosticOrderStatus', 'CAN', 'Cancelled'),
('ResultStatus', 'PREL', 'Preliminary'),
('ResultStatus', 'FIN', 'Final'),
('ResultStatus', 'AMD', 'Amended');

-- ---------- BILLING LOOKUPS ----------
INSERT INTO core.Lookup_Billing (Category, Code, Label) VALUES
('InvoiceStatus', 'DFT', 'Draft'),
('InvoiceStatus', 'ISS', 'Issued'),
('InvoiceStatus', 'PAID', 'Paid'),
('InvoiceStatus', 'PART', 'Partially Paid'),
('InvoiceStatus', 'CAN', 'Cancelled'),
('ChargeStatus', 'PEND', 'Pending'),
('ChargeStatus', 'BILL', 'Billed'),
('ChargeStatus', 'WAIV', 'Waived'),
('PaymentMethod', 'CASH', 'Cash'),
('PaymentMethod', 'CARD', 'Card'),
('PaymentMethod', 'UPI', 'UPI'),
('PaymentMethod', 'CHQ', 'Cheque'),
('PaymentMethod', 'NEFT', 'Bank Transfer'),
('PaymentMethod', 'TPA', 'TPA / Insurance Settlement'),
('ServiceCategory', 'OPD', 'OPD Consultation'),
('ServiceCategory', 'IPD', 'IPD Room/Bed'),
('ServiceCategory', 'LAB', 'Laboratory'),
('ServiceCategory', 'IMG', 'Imaging'),
('ServiceCategory', 'PHRM', 'Pharmacy'),
('ServiceCategory', 'SURG', 'Surgery'),
('ConcessionScheme', 'NONE', 'No Concession'),
('ConcessionScheme', 'STAFF', 'Staff Concession'),
('ConcessionScheme', 'CHAR', 'Charity/Concessional');

-- ---------- INVENTORY LOOKUPS ----------
INSERT INTO core.Lookup_Inventory (Category, Code, Label) VALUES
('IndentStatus', 'REQ', 'Requested'),
('IndentStatus', 'APR', 'Approved'),
('IndentStatus', 'ISS', 'Issued'),
('IndentStatus', 'REJ', 'Rejected'),
('DispenseStatus', 'DISP', 'Dispensed'),
('DispenseStatus', 'RET', 'Returned');

-- ---------- MISC LOOKUPS (legacy provdmast — genuinely ambiguous small master) ----------
INSERT INTO core.Lookup_Misc (Category, Code, Label) VALUES
('Provision', 'GEN', 'General'),
('OrganizationType', 'TPA', 'TPA / Insurer'),
('OrganizationType', 'VEND', 'Vendor / Supplier'),
('OrganizationType', 'REF', 'Referral Source');

-- ---------- SECURITY ROLES ----------
INSERT INTO security.Role (RoleName) VALUES
('Admin'), ('Doctor'), ('Nurse'), ('Receptionist'),
('Pharmacist'), ('LabTechnician'), ('BillingClerk'), ('Accountant');

-- ---------- CHART OF ACCOUNTS (minimum top-level groups) ----------
INSERT INTO finance.Account (CompanyId, AccountCode, AccountName, AccountGroup, IsControlAccount) VALUES
(1, '1000', 'Cash in Hand', 'Asset', 0),
(1, '1100', 'Bank Account', 'Asset', 0),
(1, '1200', 'Accounts Receivable - Patients', 'Asset', 0),
(1, '1210', 'Accounts Receivable - TPA/Insurance', 'Asset', 1),
(1, '2000', 'Accounts Payable - Vendors', 'Liability', 1),
(1, '2100', 'GST Payable', 'Liability', 0),
(1, '4000', 'OPD Revenue', 'Income', 0),
(1, '4100', 'IPD Revenue', 'Income', 0),
(1, '4200', 'Pharmacy Revenue', 'Income', 0),
(1, '4300', 'Diagnostic Revenue', 'Income', 0),
(1, '5000', 'Pharmacy Purchase Expense', 'Expense', 0),
(1, '5100', 'Staff Salary Expense', 'Expense', 0),
(1, '5200', 'General Operating Expense', 'Expense', 0);

-- ---------- FISCAL PERIODS (current FY, adjust dates to your fiscal calendar) ----------
INSERT INTO finance.FiscalPeriod (FiscalYear, PeriodNo, StartDate, EndDate) VALUES
('2026-2027', 1, '2026-04-01', '2026-04-30'),
('2026-2027', 2, '2026-05-01', '2026-05-31'),
('2026-2027', 3, '2026-06-01', '2026-06-30'),
('2026-2027', 4, '2026-07-01', '2026-07-31'),
('2026-2027', 5, '2026-08-01', '2026-08-31'),
('2026-2027', 6, '2026-09-01', '2026-09-30'),
('2026-2027', 7, '2026-10-01', '2026-10-31'),
('2026-2027', 8, '2026-11-01', '2026-11-30'),
('2026-2027', 9, '2026-12-01', '2026-12-31'),
('2026-2027', 10, '2027-01-01', '2027-01-31'),
('2026-2027', 11, '2027-02-01', '2027-02-28'),
('2026-2027', 12, '2027-03-01', '2027-03-31');

-- ---------- SAMPLE DEPARTMENT (at least one, since Encounter.DepartmentId
-- and Staff.PrimaryDepartmentId are referenced elsewhere) ----------
INSERT INTO core.Department (BranchId, DepartmentName, DepartmentType) VALUES
(1, 'General Medicine', 'Clinical'),
(1, 'Pharmacy', 'Pharmacy'),
(1, 'Laboratory', 'Diagnostic'),
(1, 'Administration', 'Admin');

-- ---------- SAMPLE SERVICE (core.Service — needed before TariffRate/TestProfile can reference it) ----------
INSERT INTO core.Service (BranchId, ServiceCode, ServiceName, ServiceType) VALUES
(1, 'OPD-CONS', 'OPD Consultation', 'Clinical'),
(1, 'LAB-GEN', 'General Laboratory', 'Diagnostic'),
(1, 'IMG-GEN', 'General Imaging', 'Diagnostic');
