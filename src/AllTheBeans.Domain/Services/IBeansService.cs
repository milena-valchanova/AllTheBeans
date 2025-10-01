using AllTheBeans.Domain.DataModels;

namespace AllTheBeans.Domain.Services;

public interface IBeansService
{
    Task<List<IBeanDTO>> GetAllAsync(
        IGetAllParameters getAllParameters, 
        CancellationToken cancellationToken = default);
    
    Task<int> CountAllAsync(ISearchParameters searchParameters, CancellationToken cancellationToken = default);

    Task<IBeanDTO> GetOrCreateBeanOfTheDayAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<IBeanDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IBeanDTO> CreateAsync(ICreateOrUpdateBeanDTO beanDTO, CancellationToken cancellationToken = default);

    Task<IBeanDTO> CreateOrUpdateAsync(Guid id, ICreateOrUpdateBeanDTO beanDTO, CancellationToken cancellationToken = default);

    Task DeleteBeanAsync(Guid id, CancellationToken cancellationToken = default);
}
