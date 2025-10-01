using AllTheBeans.Domain.Entities;

namespace AllTheBeans.Domain.Repositories;
public interface ICountriesRepository
{
    Task<Country> GetOrCreate(string name, CancellationToken cancellationToken = default);

    Task<Country> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Country country, CancellationToken cancellationToken = default);
}
