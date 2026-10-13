using Dapper;
using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MedCare.Infrastructure.Repositories.NewFolder.Core;

public class NationalityRepository(ISqlConnectionFactory connectionFactory, ILogger<NationalityRepository> logger) : INationalityRepository
{
    public async Task<int> DataProcessAsync(int action, NationalityReqDTO command, int nationality, int userId)
    {
        var actionName = action switch
        {
            1 => "Insert Nationality",
            2 => "Update Nationality",
            3 => "Delete Nationality",
            _ => "Unknown Action"
        };

        using var connection = connectionFactory.CreateConnection();

        var dp = new DynamicParameters();
        dp.Add("@Action", action, DbType.Int32);
        dp.Add("@NationalityId", nationality, DbType.Int32);
        dp.Add("@NationalityName", command.NationalityName, DbType.String, ParameterDirection.Input, 100);
        dp.Add("@IsActive", command.IsActive ? 1:0, DbType.Boolean, ParameterDirection.Input);

        dp.Add("@error", dbType: DbType.Int32, direction: ParameterDirection.Output);
        dp.Add("@OutID", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("core.[usp_Nationality]", dp, commandType: CommandType.StoredProcedure);

        var error = dp.Get<int>("@error");
        var outID = dp.Get<int>("@OutID");

        if (error != 0)
        {
            logger.LogError(
                "Stored Procedure Error: {Action} failed with error code {ErrorCode}. " +
                "NationalityID: {NationalityId}, OutID: {OutID}, UserID: {UserId}",
                actionName, error, nationality, outID, userId);
        }

        return outID;
    }

    public async Task<NationalityReqDTO?> GetByIdAsync(int nationalityId)
    {
        const string sql = @"SELECT NationalityId, NationalityName, IsActive FROM core.Nationality WHERE NationalityId=@nationalityId";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<NationalityReqDTO>(sql, new { NationalityId = nationalityId });
            logger.LogInformation("Retrieved nationality {NationalityId}", nationalityId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving nationality {NationalityId}", nationalityId);
            throw;
        }
    }

    public async Task<PagedResult<NationalityRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct)
    {
        var sortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["nationality"] = "NationalityName"
        };

        var sortColumn = sortColumns.GetValueOrDefault(query.SortBy ?? "", "NationalityName");
        var direction = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        var sql = $@"
                SELECT NationalityId, NationalityName, IsActive, COUNT(*) OVER() AS TotalCount FROM core.Nationality WHERE @Search IS NULL OR NationalityName LIKE @Search 
                ORDER BY {sortColumn} {direction}, NationalityId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY";

        var args = new
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"{query.Search.Trim()}%",
            Skip = (query.Page - 1) * query.PageSize,
            query.PageSize
        };

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var rows = (await connection.QueryAsync<NationalityRespDTO, int, (NationalityRespDTO Item, int Total)>(
                new CommandDefinition(sql, args, cancellationToken: ct),
                (item, total) => (item, total),
                splitOn: "TotalCount")).AsList();

            var total = rows.Count > 0 ? rows[0].Total : 0;
            logger.LogInformation("Retrieved {Count} nationality (page {Page}, total {Total})", rows.Count, query.Page, total);

            return new PagedResult<NationalityRespDTO>(rows.ConvertAll(r => r.Item), total, query.Page, query.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving nationality for page {Page}", query.Page);
            throw;
        }
    }

    public async Task<bool> IsDuplicateAsync(NationalityReqDTO nationality, int nationalityId)
    {
        const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM core.Nationality WHERE UPPER(NationalityName) = UPPER(@NationalityName) AND NationalityId !=@nationalityId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var isDuplicate = await connection.ExecuteScalarAsync<bool>(sql, new { nationality.NationalityName, nationalityId });
            if (isDuplicate)
            {
                logger.LogWarning("Duplicate Nationality detected: NationalityName={NationalityName}",
                    nationality.NationalityName);
            }
            return isDuplicate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking duplicate nationality");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int nationalityId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM core.Nationality WHERE NationalityId = @NationalityId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { NationalityId = nationalityId });
            logger.LogDebug("Patient {NationalityId} exists: {Exists}", nationalityId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking nationality existence {NationalityId}", nationalityId);
            throw;
        }
    }
}
