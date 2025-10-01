using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace AllTheBeans.Domain.Services.Implementation;

internal class BeansService(
    BeansContext _context,
    IBeansRepository _beansRepository, 
    ICountriesRepository _countriesRepository) : IBeansService
{
    private readonly Expression<Func<Bean, IBeanDTO>> _beanSelector = p => new BeanDTO()
    {
        Id = p.Id,
        Index = p.Index,
        IsBOTD = p.IsBOTD,
        Cost = p.Cost,
        ImageName = p.ImageName,
        Colour = p.Colour,
        Name = p.Name,
        Description = p.Description,
        CountryName = p.Country.Name
    } as IBeanDTO;

    public Task<List<IBeanDTO>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        => _beansRepository.GetAll(pageNumber, pageSize, cancellationToken)
        .Select(_beanSelector)
        .ToListAsync(cancellationToken);

    public Task<int> CountAllAsync(CancellationToken cancellationToken = default)
        => _beansRepository.CountAllAsync(cancellationToken);

    public async Task<IBeanDTO> CreateAsync(ICreateOrUpdateBeanDTO beanDTO, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var country = await _countriesRepository.GetOrCreateAsync(beanDTO.CountryName, cancellationToken);
        var beanId = await _beansRepository.CreateAsync(beanDTO, country.Id, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetByIdAsync(beanId, cancellationToken);
    }

    public async Task<IBeanDTO> CreateOrUpdateAsync(Guid id, ICreateOrUpdateBeanDTO beanDTO, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var bean = await _beansRepository.GetByIdOrDefaultAsync(id, cancellationToken);
        var country = await _countriesRepository.GetOrCreateAsync(beanDTO.CountryName, cancellationToken);
        Guid result;
        if (bean is null)
        {
            result = await _beansRepository.CreateAsync(beanDTO, country.Id, cancellationToken);
        }
        else
        {
            result = bean.Id;
            await _beansRepository.UpdateAsync(bean, beanDTO, country.Id, cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
        return await GetByIdAsync(result, cancellationToken);
    }

    public async Task DeleteBeanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var bean = await _beansRepository.GetByIdAsync(id, cancellationToken);
        var countryId = bean.CountryId;
        await _beansRepository.DeleteAsync(bean, cancellationToken);

        var country = await _countriesRepository.GetByIdAsync(countryId, cancellationToken);
        if (country.Beans.Count == 0)
        {
            await _countriesRepository.DeleteAsync(country, cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IBeanDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetAllAsync(1, 1, cancellationToken);
        return result
            .FirstOrDefault()
            ?? throw new KeyNotFoundException($"Bean with id {id} was not found");
    }
}
