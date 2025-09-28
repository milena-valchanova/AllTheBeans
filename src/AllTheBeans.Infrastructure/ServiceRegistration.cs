using AllTheBeans.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace AllTheBeans.Infrastructure;
public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var beansDbConnectionStringName = "BeansDbConnectionString";
        var connectionString = configuration
            .GetConnectionString(beansDbConnectionStringName)
            ?? throw new ArgumentException($"Connection string {beansDbConnectionStringName} must be provided.");

        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        services.AddDbContext<BeansContext>(options =>
        {
            options.UseNpgsql(dataSource, npgSqlOptions =>
            {
                npgSqlOptions.MigrationsAssembly(typeof(ServiceRegistration).Assembly.FullName);
            });
        });
        return services;
    }
}
