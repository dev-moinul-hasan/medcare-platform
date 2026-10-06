using MedCare.Application.Abstractions.Services;
using MedCare.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MedCare.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPatientService, PatientService>();
            return services;
        }
    }
}
