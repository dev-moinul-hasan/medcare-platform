using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories;
using MedCare.Infrastructure.Data;
using MedCare.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MedCare.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // 1. Connection Factory
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

            // 2. Repositories
            services.AddScoped<IPatientRepository, PatientRepository>();

            return services;
        }
    }
}
