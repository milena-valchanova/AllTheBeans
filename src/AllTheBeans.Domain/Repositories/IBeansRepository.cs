using AllTheBeans.Domain.DataModels;

namespace AllTheBeans.Domain.Repositories;
public interface IBeansRepository
{
    Task<Guid> CreateAsync(IBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default);
}
