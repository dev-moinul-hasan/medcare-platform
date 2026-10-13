using Dapper;
using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MedCare.Infrastructure.Repositories.NewFolder.Core;

public class ReligionRepository(ISqlConnectionFactory connectionFactory, ILogger<ReligionRepository> logger) : IReligionRepository
{
    public async Task<int> DataProcessAsync(int action, ReligionReqDTO command, int religion, int userId)
    {
        var actionName = action switch
        {
            1 => "Insert Religion",
            2 => "Update Religion",
            3 => "Delete Religion",
            _ => "Unknown Action"
        };

        using var connection = connectionFactory.CreateConnection();

        var dp = new DynamicParameters();
        dp.Add("@Action", action, DbType.Int32);
        dp.Add("@ReligionId", religion, DbType.Int32);
        dp.Add("@ReligionName", command.ReligionName, DbType.String, ParameterDirection.Input, 100);
        dp.Add("@IsActive", command.IsActive ? 1:0, DbType.Boolean, ParameterDirection.Input);

        dp.Add("@error", dbType: DbType.Int32, direction: ParameterDirection.Output);
        dp.Add("@OutID", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("core.[usp_Religion]", dp, commandType: CommandType.StoredProcedure);

        var error = dp.Get<int>("@error");
        var outID = dp.Get<int>("@OutID");

        if (error != 0)
        {
            logger.LogError(
                "Stored Procedure Error: {Action} failed with error code {ErrorCode}. " +
                "ReligionID: {ReligionId}, OutID: {OutID}, UserID: {UserId}",
                actionName, error, religion, outID, userId);
        }

        return outID;
    }

    public async Task<ReligionReqDTO?> GetByIdAsync(int religionId)
    {
        const string sql = @"SELECT ReligionId, ReligionName, IsActive FROM core.Religion WHERE ReligionId=@religionId";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<ReligionReqDTO>(sql, new { ReligionId = religionId });
            logger.LogInformation("Retrieved religion {ReligionId}", religionId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving religion {ReligionId}", religionId);
            throw;
        }
    }

    public async Task<PagedResult<ReligionRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct)
    {
        var sortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["religion"] = "ReligionName"
        };

        var sortColumn = sortColumns.GetValueOrDefault(query.SortBy ?? "", "ReligionName");
        var direction = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        var sql = $@"
                SELECT ReligionId, ReligionName, IsActive, COUNT(*) OVER() AS TotalCount FROM core.Religion WHERE @Search IS NULL OR ReligionName LIKE @Search 
                ORDER BY {sortColumn} {direction}, ReligionId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY";

        var args = new
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"{query.Search.Trim()}%",
            Skip = (query.Page - 1) * query.PageSize,
            query.PageSize
        };

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var rows = (await connection.QueryAsync<ReligionRespDTO, int, (ReligionRespDTO Item, int Total)>(
                new CommandDefinition(sql, args, cancellationToken: ct),
                (item, total) => (item, total),
                splitOn: "TotalCount")).AsList();

            var total = rows.Count > 0 ? rows[0].Total : 0;
            logger.LogInformation("Retrieved {Count} religion (page {Page}, total {Total})", rows.Count, query.Page, total);

            return new PagedResult<ReligionRespDTO>(rows.ConvertAll(r => r.Item), total, query.Page, query.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving religion for page {Page}", query.Page);
            throw;
        }
    }

    public async Task<bool> IsDuplicateAsync(ReligionReqDTO religion, int religionId)
    {
        const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM core.Religion WHERE UPPER(ReligionName) = UPPER(@ReligionName) AND ReligionId !=@religionId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var isDuplicate = await connection.ExecuteScalarAsync<bool>(sql, new { religion.ReligionName, religionId });
            if (isDuplicate)
            {
                logger.LogWarning("Duplicate Religion detected: ReligionName={ReligionName}",
                    religion.ReligionName);
            }
            return isDuplicate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking duplicate religion");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int religionId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM core.Religion WHERE ReligionId = @ReligionId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { ReligionId = religionId });
            logger.LogDebug("Patient {ReligionId} exists: {Exists}", religionId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking religion existence {ReligionId}", religionId);
            throw;
        }
    }
}
