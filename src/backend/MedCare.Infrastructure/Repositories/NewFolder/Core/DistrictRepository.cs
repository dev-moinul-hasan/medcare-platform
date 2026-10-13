using Dapper;
using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MedCare.Infrastructure.Repositories.NewFolder.Core;

public class DistrictRepository(ISqlConnectionFactory connectionFactory, ILogger<DistrictRepository> logger) : IDistrictRepository
{
    public async Task<int> DataProcessAsync(int action, DistrictReqDTO command, int districtId, int userId)
    {
        var actionName = action switch
        {
            1 => "Insert District",
            2 => "Update District",
            3 => "Delete District",
            _ => "Unknown Action"
        };

        using var connection = connectionFactory.CreateConnection();

        var dp = new DynamicParameters();
        dp.Add("@Action", action, DbType.Int32);
        dp.Add("@DistrictId", districtId, DbType.Int32);
        dp.Add("@DistrictName", command.DistrictName, DbType.String, ParameterDirection.Input, 100);
        dp.Add("@StateId", command.StateId, DbType.Int32, ParameterDirection.Input);
        dp.Add("@IsActive", command.IsActive ? 1:0, DbType.Boolean, ParameterDirection.Input);

        dp.Add("@error", dbType: DbType.Int32, direction: ParameterDirection.Output);
        dp.Add("@OutID", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("core.[usp_District]", dp, commandType: CommandType.StoredProcedure);

        var error = dp.Get<int>("@error");
        var outID = dp.Get<int>("@OutID");

        if (error != 0)
        {
            logger.LogError(
                "Stored Procedure Error: {Action} failed with error code {ErrorCode}. " +
                "DistrictID: {DistrictId}, OutID: {OutID}, UserID: {UserId}",
                actionName, error, districtId, outID, userId);
        }

        return outID;
    }

    public async Task<DistrictReqDTO?> GetByIdAsync(int districtId)
    {
        const string sql = @"SELECT DistrictId, DistrictName, StateId, IsActive FROM core.District WHERE DistrictId=@districtId";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<DistrictReqDTO>(sql, new { DistrictId = districtId });
            logger.LogInformation("Retrieved district {DistrictId}", districtId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving district {DistrictId}", districtId);
            throw;
        }
    }

    public async Task<PagedResult<DistrictRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct)
    {
        var sortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["districtName"] = "d.DistrictName",
            ["stateName"] = "s.StateName"
        };

        var sortColumn = sortColumns.GetValueOrDefault(query.SortBy ?? "", "DistrictName");
        var direction = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        var sql = $@"
                SELECT d.DistrictId, d.DistrictName, s.StateId, d.IsActive, COUNT(*) OVER() AS TotalCount FROM core.District d INNER JOIN core.State s ON d.StateId = s.StateId WHERE @Search IS NULL OR d.DistrictName LIKE @Search OR s.StateName  LIKE @Search
                ORDER BY {sortColumn} {direction}, d.DistrictId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY";

        var args = new
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"{query.Search.Trim()}%",
            Skip = (query.Page - 1) * query.PageSize,
            query.PageSize
        };

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var rows = (await connection.QueryAsync<DistrictRespDTO, int, (DistrictRespDTO Item, int Total)>(
                new CommandDefinition(sql, args, cancellationToken: ct),
                (item, total) => (item, total),
                splitOn: "TotalCount")).AsList();

            var total = rows.Count > 0 ? rows[0].Total : 0;
            logger.LogInformation("Retrieved {Count} district (page {Page}, total {Total})", rows.Count, query.Page, total);

            return new PagedResult<DistrictRespDTO>(rows.ConvertAll(r => r.Item), total, query.Page, query.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving district for page {Page}", query.Page);
            throw;
        }
    }

    public async Task<bool> IsDuplicateAsync(DistrictReqDTO district, int districtId)
    {
        const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM core.District WHERE UPPER(DistrictName) = UPPER(@DistrictName) And DistrictId != @districtId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var isDuplicate = await connection.ExecuteScalarAsync<bool>(sql, new { district.DistrictName, districtId });
            if (isDuplicate)
            {
                logger.LogWarning("Duplicate District detected: DistrictName={DistrictName}",
                    district.DistrictName);
            }
            return isDuplicate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking duplicate district");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int districtId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM core.District WHERE DistrictId = @DistrictId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { DistrictId = districtId });
            logger.LogDebug("Patient {DistrictId} exists: {Exists}", districtId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking district existence {DistrictId}", districtId);
            throw;
        }
    }
}
