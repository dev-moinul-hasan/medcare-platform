using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories;
using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Infrastructure.Data;
using MedCare.Infrastructure.Repositories;
using MedCare.Infrastructure.Repositories.NewFolder.Core;
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

            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IStateRepository, StateRepository>();
            services.AddScoped<IDistrictRepository, DistrictRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<INationalityRepository, NationalityRepository>();
            services.AddScoped<IReligionRepository, ReligionRepository>();
            services.AddScoped<IOccupationRepository, OccupationRepository>();
            services.AddScoped<IIDTypeRepository, IDTypeRepository>();

            return services;
        }
    }
}
