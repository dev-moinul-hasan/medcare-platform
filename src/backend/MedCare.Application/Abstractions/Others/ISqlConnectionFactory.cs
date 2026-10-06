using System.Data;

namespace MedCare.Application.Abstractions.Others;
public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}

