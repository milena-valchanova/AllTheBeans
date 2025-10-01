using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;

namespace AllTheBeans.Domain.Repositories;
public interface IBeansRepository
{
    Task<List<IBeanDTO>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAllAsync(CancellationToken cancellationToken = default);

    Task<IBeanDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Bean> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Bean bean, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(ICreateBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default);

}
