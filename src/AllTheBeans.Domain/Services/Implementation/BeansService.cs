using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AllTheBeans.Domain.Services.Implementation;

internal class BeansService(
    BeansContext _context,
    IBeansRepository _beansRepository, 
    ICountriesRepository _countriesRepository) : IBeansService
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

    public async Task DeleteBeanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var bean = await _beansRepository.GetByIdTrackedAsync(id, cancellationToken);
        var countryId = bean.CountryId;
        await _beansRepository.DeleteAsync(bean, cancellationToken);

        var country = await _countriesRepository.GetByIdAsync(countryId, cancellationToken);
        if (country.Beans.Count == 0)
        {
            await _countriesRepository.DeleteAsync(country, cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }
}
