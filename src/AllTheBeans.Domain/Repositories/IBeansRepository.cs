using AllTheBeans.Domain.DataModels;

namespace AllTheBeans.Domain.Repositories;
public interface IBeansRepository
{
    Task<List<IBeanDTO>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAllAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(ICreateBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default);

}
