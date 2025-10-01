using AllTheBeans.Domain.DataModels;

namespace AllTheBeans.Domain.Services;

public interface IBeansService
{
    Task<Guid> InitiliseAsync(ICreateBeanDTO beanDTO, CancellationToken cancellationToken = default);

    Task DeleteBeanAsync(Guid id, CancellationToken cancellationToken = default);
}
