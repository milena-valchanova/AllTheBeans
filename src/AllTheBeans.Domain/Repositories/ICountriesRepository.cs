using AllTheBeans.Domain.Entities;

namespace AllTheBeans.Domain.Repositories;
internal interface ICountriesRepository
{
    Task<Country> GetOrCreateAsync(string name, CancellationToken cancellationToken = default);

    Task<Country> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Country country, CancellationToken cancellationToken = default);
}
