using AllTheBeans.Domain.Entities;

namespace AllTheBeans.Domain.Repositories;
internal interface ICountriesRepository
{
    Task<Country> GetOrCreate(string name, CancellationToken cancellationToken = default);
}
