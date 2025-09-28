using AllTheBeans.Domain.DataModels;

namespace AllTheBeans.Domain.Repositories;
public interface IBeansRepository
{
    Task CreateAsync(IBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default);
}
