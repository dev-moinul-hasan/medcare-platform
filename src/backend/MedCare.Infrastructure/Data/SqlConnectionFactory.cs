using Microsoft.Data.SqlClient;
using MedCare.Application.Abstractions.Others;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace MedCare.Infrastructure.Data;

internal class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string was not found.");

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
