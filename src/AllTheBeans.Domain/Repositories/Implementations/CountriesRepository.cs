using AllTheBeans.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

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

    public async Task<Country> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _context.Countries
            .Include(p => p.Beans)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        ?? throw new KeyNotFoundException($"Country with id {id} was not found");

    public async Task DeleteAsync(Country country, CancellationToken cancellationToken = default)
    {
        _context.Countries.Remove(country);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
