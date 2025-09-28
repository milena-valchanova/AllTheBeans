using AllTheBeans.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AllTheBeans.Infrastructure;
internal class BeansContextDesignTimeFactory : IDesignTimeDbContextFactory<BeansContext>
{
    public BeansContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BeansContext>();
        optionsBuilder.UseNpgsql(npgSqlOptions =>
        {
            npgSqlOptions.MigrationsAssembly(typeof(ServiceRegistration).Assembly.FullName);
        });
        return new BeansContext(optionsBuilder.Options);
    }
}
