using AllTheBeans.Domain.Entities;

namespace AllTheBeans.Domain.Repositories;
public interface ICountriesRepository
{
    Task<Country> GetOrCreate(string name, CancellationToken cancellationToken = default);
}
