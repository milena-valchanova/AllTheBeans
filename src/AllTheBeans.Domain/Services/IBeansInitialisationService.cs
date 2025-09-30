using AllTheBeans.Domain.DataModels;

namespace AllTheBeans.Domain.Services;

public interface IBeansInitialisationService
{
    Task<Guid> InitiliseAsync(IBeanDTO beanDTO, CancellationToken cancellationToken = default);
}
