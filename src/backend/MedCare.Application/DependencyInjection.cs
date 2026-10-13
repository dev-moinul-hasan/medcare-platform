using MedCare.Application.Abstractions.Services;
using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.Services;
using MedCare.Application.Services.Core;
using Microsoft.Extensions.DependencyInjection;

namespace MedCare.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<IDistrictService, DistrictService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<INationalityService, NationalityService>();
            services.AddScoped<IReligionService, ReligionService>();
            services.AddScoped<IOccupationService, OccupationService>();
            services.AddScoped<IIDTypeService, IDTypeService>();
            return services;
        }
    }
}
