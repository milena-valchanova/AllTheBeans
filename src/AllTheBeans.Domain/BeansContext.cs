using AllTheBeans.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AllTheBeans.Domain;
public class BeansContext(DbContextOptions<BeansContext> options) : DbContext(options)
{
    public DbSet<Bean> Beans { get; set; }
    public DbSet<Country> Countries { get; set; }
}
