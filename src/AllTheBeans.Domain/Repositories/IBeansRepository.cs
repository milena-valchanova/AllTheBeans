using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;

namespace AllTheBeans.Domain.Repositories;
internal interface IBeansRepository
{
    IQueryable<Bean> GetAll(IGetAllParameters getAllParameters);

    Task<int> CountAllAsync(ISearchParameters searchParameters, CancellationToken cancellationToken = default);

    Task<Bean> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Bean?> GetByIdOrDefaultAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Bean bean, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(ICreateOrUpdateBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Guid beanId, ICreateOrUpdateBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default);
}
