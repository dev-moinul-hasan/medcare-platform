using Dapper;
using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Party;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MedCare.Infrastructure.Repositories
{
    public class PatientRepository(ISqlConnectionFactory connectionFactory, ILogger<PatientRepository> logger) : IPatientRepository
    {
        public async Task<int> DataProcessAsync(int action, PatientReqDTO command, int patientId, int userId)
        {
            var actionName = action switch
            {
                1 => "Insert Patient",
                2 => "Update Patient",
                3 => "Delete Patient",
                _ => "Unknown Action"
            };

            using var connection = connectionFactory.CreateConnection();

            var dp = new DynamicParameters();
            dp.Add("@Action", action, DbType.Int32);
            dp.Add("@PatientId", patientId, DbType.Int32);
            dp.Add("@FirstName", command.FirstName, DbType.String, ParameterDirection.Input, 100);
            dp.Add("@MiddleName", command.MiddleName, DbType.String, ParameterDirection.Input, 100);
            dp.Add("@LastName", command.LastName, DbType.String, ParameterDirection.Input, 100);
            dp.Add("@DateOfBirth", command.DateOfBirth, DbType.Date);
            dp.Add("@GenderId", command.GenderId, DbType.Int32);
            dp.Add("@MobileNo", command.MobileNo, DbType.String, ParameterDirection.Input, 20);
            dp.Add("@Email", command.Email, DbType.String, ParameterDirection.Input, 20);
            dp.Add("@Address", command.Address, DbType.String, ParameterDirection.Input, 200);
            dp.Add("@City", command.City, DbType.String, ParameterDirection.Input, 100);
            dp.Add("@PinCode", command.PinCode, DbType.String, ParameterDirection.Input, 20);

            dp.Add("@DistrictId", command.DistrinctId, DbType.Int32);
            dp.Add("@StateId", command.StateId, DbType.Int32);
            dp.Add("@CountryId", command.CountryId, DbType.Int32);
            dp.Add("@RelagionId", command.RelagionId, DbType.Int32);
            dp.Add("@PrimaryBranchId", command.PrimaryBranchId, DbType.Int32);

            dp.Add("@BloodGroupId", command.BloodGroupId, DbType.Int32);
            dp.Add("@OccupationId", command.OccoupationId, DbType.Int32);
            dp.Add("@MaritalStatusId", command.MaritalStatus, DbType.Int32); 
            dp.Add("@RegFees", command.RegFees, DbType.Decimal);
            dp.Add("@UserId", userId, DbType.Int32);
            dp.Add("@error", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@OutID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("party.[usp_Patient]", dp, commandType: CommandType.StoredProcedure);

            var error = dp.Get<int>("@error");
            var outID = dp.Get<int>("@OutID");

            if (error != 0)
            {
                logger.LogError(
                    "Stored Procedure Error: {Action} failed with error code {ErrorCode}. " +
                    "PatientID: {PatientId}, OutID: {OutID}, UserID: {UserId}",
                    actionName, error, patientId, outID, userId);
            }

            return outID;
        }

        public async Task<PatientReqDTO?> GetPatientByIdAsync(int patientId)
        {
            const string sql = @"SELECT pe.PartyId, pe.FirstName, pe.MiddleName, pe.LastName, pe.DateOfBirth, pe.GenderId, pe.MobileNo, pe.Address, pe.City, pe.PinCode, pe.DistrictId, pe.StateId, pe.CountryId, pe.ReligionId pa.BloodGroupId, pa.OccupationId, pa.MaritalStatusId,b.Amount FROM party.Person pe
                                LEFT JOIN party.Patient pa ON pe.PartyId = pa.PatientId WHERE pe.PartyId = @PatientId LEFT JOIN billin.Charge  b ON pa.PatientId = b.PatientId AND ServiceId = 2501";

            using var connection = connectionFactory.CreateConnection();

            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<PatientReqDTO>(sql, new { PatientId = patientId });
                logger.LogInformation("Retrieved patient {PatientId}", patientId);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<PagedResult<PatientReqDTO>> GetPatientByPageAsync(PageQuery query, CancellationToken ct)
        {
            var sortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["name"] = "pe.LastName",
                ["dob"] = "pe.DateOfBirth",
                ["mobile"] = "pe.MobileNo",
            };

            var sortColumn = sortColumns.GetValueOrDefault(query.SortBy ?? "", "pe.LastName");
            var direction = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            var sql = $@"
                SELECT pe.FirstName, pe.MiddleName, pe.LastName, pe.DateOfBirth,  pe.GenderId, pe.MobileNo, pe.NationalId, pa.BloodGroupId, COUNT(*) OVER() AS TotalCount
                FROM party.Person pe  LEFT JOIN party.Patient pa ON pa.PatientId = pe.PartyId WHERE @Search IS NULL OR pe.FirstName LIKE @Search OR pe.LastName LIKE @Search OR pe.MobileNo LIKE @Search
                ORDER BY {sortColumn} {direction}, pe.PartyId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY";

            var args = new
            {
                Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"{query.Search.Trim()}%",
                Skip = (query.Page - 1) * query.PageSize,
                query.PageSize
            };

            using var connection = connectionFactory.CreateConnection();

            try
            {
                var rows = (await connection.QueryAsync<PatientReqDTO, int, (PatientReqDTO Item, int Total)>(
                    new CommandDefinition(sql, args, cancellationToken: ct),
                    (item, total) => (item, total),
                    splitOn: "TotalCount")).AsList();

                var total = rows.Count > 0 ? rows[0].Total : 0;
                logger.LogInformation("Retrieved {Count} patients (page {Page}, total {Total})", rows.Count, query.Page, total);

                return new PagedResult<PatientReqDTO>(rows.ConvertAll(r => r.Item), total, query.Page, query.PageSize);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving patients for page {Page}", query.Page);
                throw;
            }
        }

        public async Task<bool> IsDuplicateAsync(PatientReqDTO patient)
        {
            const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM party.Person WHERE MobileNo = @MobileNo OR (@CountryId IS NOT NULL AND CountryId = @CountryId)) THEN 1 ELSE 0 END";

            using var connection = connectionFactory.CreateConnection();

            try
            {
                var isDuplicate = await connection.ExecuteScalarAsync<bool>(sql, new { patient.MobileNo, patient.CountryId });
                if (isDuplicate)
                {
                    logger.LogWarning("Duplicate patient detected: Mobile={Mobile}, NationalId={NationalId}",
                        patient.MobileNo, patient.CountryId);
                }
                return isDuplicate;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking duplicate patient");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int patientId)
        {
            const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM party.Patient WHERE PatientId = @PatientId) THEN 1 ELSE 0 END";

            using var connection = connectionFactory.CreateConnection();

            try
            {
                var exists = await connection.ExecuteScalarAsync<bool>(sql, new { PatientId = patientId });
                logger.LogDebug("Patient {PatientId} exists: {Exists}", patientId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking patient existence {PatientId}", patientId);
                throw;
            }
        }
    }
}