using AllTheBeans.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AllTheBeans.Domain.Repositories.Implementations;
internal class CountriesRepository(BeansContext _context) : ICountriesRepository
{
    public async Task<Country> GetOrCreate(string name, CancellationToken cancellationToken = default)
    {
        var country = await _context.Countries
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
        if (country is not null)
        {
            return country;
        }
        var newCountry = new Country
        {
            Name = name
        };
        _context.Countries.Add(newCountry);
        await _context.SaveChangesAsync(cancellationToken);

        return newCountry;
    }
}
