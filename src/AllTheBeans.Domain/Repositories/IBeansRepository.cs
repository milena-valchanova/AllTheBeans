using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;

namespace AllTheBeans.Domain.Repositories;
internal interface IBeansRepository
{
    IQueryable<Bean> GetAll(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAllAsync(CancellationToken cancellationToken = default);

    Task<Bean> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Bean?> GetByIdOrDefaultAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Bean bean, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(ICreateOrUpdateBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Bean bean, ICreateOrUpdateBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default);
}
