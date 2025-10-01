using AllTheBeans.Domain.Entities;

namespace AllTheBeans.Domain.Repositories;
internal interface IBeanOfTheDayRepository
{
    Task<BeanOfTheDay?> GetOrDefaultAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<BeanOfTheDay> CreateAsync(Guid beanId, DateOnly date, CancellationToken cancellationToken = default);
}
