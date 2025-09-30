using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AllTheBeans.Domain.Services.Implementation;

internal class BeansInitialisationService(
    BeansContext _context,
    IBeansRepository _beansRepository, 
    ICountriesRepository _countriesRepository) : IBeansInitialisationService
{
    public async Task<Guid> InitiliseAsync(ICreateBeanDTO beanDTO, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var country = await _countriesRepository.GetOrCreate(beanDTO.CountryName, cancellationToken);
        var beanId = await _beansRepository.CreateAsync(beanDTO, country.Id, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return beanId;
    }
}
