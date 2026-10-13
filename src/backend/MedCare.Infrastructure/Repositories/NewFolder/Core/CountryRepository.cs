using Dapper;
using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MedCare.Infrastructure.Repositories.NewFolder.Core;

public class CountryRepository(ISqlConnectionFactory connectionFactory, ILogger<CountryRepository> logger) : ICountryRepository
{
    public async Task<int> DataProcessAsync(int action, CountryReqDTO command, int countryId, int userId)
    {
        var actionName = action switch
        {
            1 => "Insert Country",
            2 => "Update Country",
            3 => "Delete Country",
            _ => "Unknown Action"
        };

        using var connection = connectionFactory.CreateConnection();

        var dp = new DynamicParameters();
        dp.Add("@Action", action, DbType.Int32);
        dp.Add("@CountryId", countryId, DbType.Int32);
        dp.Add("@CountryName", command.CountryName, DbType.String, ParameterDirection.Input, 100);
        dp.Add("@ISOCode", command.ISOCode, DbType.String, ParameterDirection.Input, 2);
        dp.Add("@IsActive", command.IsActive ? 1:0, DbType.Boolean, ParameterDirection.Input);

        dp.Add("@error", dbType: DbType.Int32, direction: ParameterDirection.Output);
        dp.Add("@OutID", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("core.[usp_Country]", dp, commandType: CommandType.StoredProcedure);

        var error = dp.Get<int>("@error");
        var outID = dp.Get<int>("@OutID");

        if (error != 0)
        {
            logger.LogError(
                "Stored Procedure Error: {Action} failed with error code {ErrorCode}. " +
                "CountryID: {CountryId}, OutID: {OutID}, UserID: {UserId}",
                actionName, error, countryId, outID, userId);
        }

        return outID;
    }

    public async Task<CountryReqDTO?> GetByIdAsync(int countryId)
    {
        const string sql = @"SELECT CountryId, CountryName, ISOCode, IsActive FROM core.Country WHERE CountryId=@countryId";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<CountryReqDTO>(sql, new { CountryId = countryId });
            logger.LogInformation("Retrieved country {CountryId}", countryId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving country {CountryId}", countryId);
            throw;
        }
    }

    public async Task<PagedResult<CountryRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct)
    {
        var sortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["country"] = "CountryName",
            ["isoCode"] = "ISOCode"
        };

        var sortColumn = sortColumns.GetValueOrDefault(query.SortBy ?? "", "CountryName");
        var direction = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        var sql = $@"
                SELECT CountryId, CountryName, ISOCode, IsActive, COUNT(*) OVER() AS TotalCount FROM core.Country WHERE @Search IS NULL OR CountryName LIKE @Search OR ISOCode LIKE @Search
                ORDER BY {sortColumn} {direction}, CountryId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY";

        var args = new
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"{query.Search.Trim()}%",
            Skip = (query.Page - 1) * query.PageSize,
            query.PageSize
        };

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var rows = (await connection.QueryAsync<CountryRespDTO, int, (CountryRespDTO Item, int Total)>(
                new CommandDefinition(sql, args, cancellationToken: ct),
                (item, total) => (item, total),
                splitOn: "TotalCount")).AsList();

            var total = rows.Count > 0 ? rows[0].Total : 0;
            logger.LogInformation("Retrieved {Count} country (page {Page}, total {Total})", rows.Count, query.Page, total);

            return new PagedResult<CountryRespDTO>(rows.ConvertAll(r => r.Item), total, query.Page, query.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving country for page {Page}", query.Page);
            throw;
        }
    }

    public async Task<bool> IsDuplicateAsync(CountryReqDTO country, int countryId)
    {
        const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM core.Country WHERE (UPPER(CountryName) = UPPER(@CountryName) OR ( UPPER(ISOCode) = UPPER(@ISOCode))) And CountryId != @countryId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var isDuplicate = await connection.ExecuteScalarAsync<bool>(sql, new { country.CountryName, country.ISOCode, countryId });
            if (isDuplicate)
            {
                logger.LogWarning("Duplicate Country detected: CountryName={CountryName}",
                    country.CountryName);
            }
            return isDuplicate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking duplicate country");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int countryId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM core.Country WHERE CountryId = @CountryId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { CountryId = countryId });
            logger.LogDebug("Patient {CountryId} exists: {Exists}", countryId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking country existence {CountryId}", countryId);
            throw;
        }
    }
}
