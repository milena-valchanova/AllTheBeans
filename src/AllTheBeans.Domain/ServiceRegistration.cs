using AllTheBeans.Domain.Repositories;
using AllTheBeans.Domain.Repositories.Implementations;
using AllTheBeans.Domain.Services;
using AllTheBeans.Domain.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace AllTheBeans.Domain;
public static class ServiceRegistration
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IBeansRepository, BeansRepository>();
        services.AddScoped<ICountriesRepository, CountriesRepository>();
        services.AddScoped<IBeansInitialisationService, BeansInitialisationService>();
        return services;
    }
}
