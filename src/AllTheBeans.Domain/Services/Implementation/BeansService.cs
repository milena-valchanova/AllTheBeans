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
    ICountriesRepository _countriesRepository,
    IBeanOfTheDayRepository _beanOfTheDayRepository) : IBeansService
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

    public Task<List<IBeanDTO>> GetAllAsync(IGetAllParameters getAllParameters, CancellationToken cancellationToken = default)
        => _beansRepository.GetAll(getAllParameters)
        .Select(_beanSelector)
        .ToListAsync(cancellationToken);

    public Task<int> CountAllAsync(ISearchParameters searchParameters,CancellationToken cancellationToken = default)
        => _beansRepository.CountAllAsync(searchParameters, cancellationToken);

    public async Task<IBeanDTO> GetOrCreateBeanOfTheDayAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var beanOfTheDay = await _beanOfTheDayRepository.GetOrDefaultAsync(date, cancellationToken);
            if (beanOfTheDay is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                return await GetByIdAsync(beanOfTheDay.BeanId, cancellationToken);
            }
            var previousDay = date.AddDays(-1);
            var previousDayBeanOfTheDay = await _beanOfTheDayRepository
                .GetOrDefaultAsync(previousDay, cancellationToken);

            var allSelectableBeans = _beansRepository
                .GetAll(new GetAllResultsParameters());
            if (previousDayBeanOfTheDay is not null)
            {
                allSelectableBeans = allSelectableBeans
                    .Where(p => p.Id != previousDayBeanOfTheDay.BeanId);
            }

            var selectableBeanIds = await allSelectableBeans
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
            if (selectableBeanIds.Count == 0)
            {
                throw new KeyNotFoundException("There are not enough beans to select Bean Of The Day");
            }
            var random = new Random();
            int randomBeanIndex = random.Next(0, selectableBeanIds.Count);
            var selectRandomBeanId = selectableBeanIds[randomBeanIndex];
            _ = await _beanOfTheDayRepository.CreateAsync(selectRandomBeanId, date, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return await GetByIdAsync(selectRandomBeanId, cancellationToken);
        });
    }

    public async Task<IBeanDTO> CreateAsync(ICreateOrUpdateBeanDTO beanDTO, CancellationToken cancellationToken = default)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var country = await _countriesRepository.GetOrCreateAsync(beanDTO.CountryName, cancellationToken);
            var beanId = await _beansRepository.CreateAsync(beanDTO, country.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return await GetByIdAsync(beanId, cancellationToken);
        });
    }

    public async Task<IBeanDTO> CreateOrUpdateAsync(Guid id, ICreateOrUpdateBeanDTO beanDTO, CancellationToken cancellationToken = default)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
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
                await _beansRepository.UpdateAsync(bean.Id, beanDTO, country.Id, cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
            return await GetByIdAsync(result, cancellationToken);
        });
    }

    public async Task UpdateAsync(Guid id, IUpdateBeanDTO beanDTO, CancellationToken cancellationToken = default)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var country = beanDTO.CountryName is null
                ? null
                : await _countriesRepository.GetOrCreateAsync(beanDTO.CountryName, cancellationToken);
            await _beansRepository.UpdateAsync(id, beanDTO, country?.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    public async Task DeleteBeanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
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
        });
    }

    public async Task<IBeanDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parameters = new GetAllResultsParameters();
        var result = await _beansRepository.GetAll(parameters)
            .Where(p => p.Id == id)
            .Select(_beanSelector)
            .FirstOrDefaultAsync(cancellationToken);
        return result
            ?? throw new KeyNotFoundException($"Bean with id {id} was not found");
    }
}
