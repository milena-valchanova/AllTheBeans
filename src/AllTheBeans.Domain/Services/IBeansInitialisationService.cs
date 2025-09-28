using AllTheBeans.Domain.DataModels;

namespace AllTheBeans.Domain.Services;

internal interface IBeansInitialisationService
{
    Task<Guid> InitiliseAsync(IBeanDTO beanDTO, CancellationToken cancellationToken = default);
}
