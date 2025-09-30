using AllTheBeans.Domain.DataModels;

namespace AllTheBeans.Domain.Services;

public interface IBeansInitialisationService
{
    Task<Guid> InitiliseAsync(ICreateBeanDTO beanDTO, CancellationToken cancellationToken = default);
}
