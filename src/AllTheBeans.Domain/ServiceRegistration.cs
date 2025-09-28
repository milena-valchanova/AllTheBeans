using AllTheBeans.Domain.Repositories;
using AllTheBeans.Domain.Repositories.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace AllTheBeans.Domain;
public static class ServiceRegistration
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IBeansRepository, BeansRepository>();
        services.AddScoped<ICountriesRepository, CountriesRepository>();
        return services;
    }
}
