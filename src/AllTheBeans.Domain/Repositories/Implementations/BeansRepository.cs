using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Threading;

namespace AllTheBeans.Domain.Repositories.Implementations;
internal class BeansRepository(BeansContext _context) : IBeansRepository
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
    {
        if (pageNumber <= 0)
            throw new ArgumentException($"{nameof(pageNumber)} must have positive value");
        if (pageSize <= 0)
            throw new ArgumentException($"{nameof(pageSize)} must have positive value");

        var entitiesToSkip = (pageNumber - 1) * pageSize;
        return GetBeansWithCountries()
            .Select(_beanSelector)
            .OrderBy(p => p.Id)
            .Skip(entitiesToSkip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Bean> GetBeansWithCountries()
        => _context.Beans
            .Include(p => p.Country)
            .AsNoTracking();

    public Task<int> CountAllAsync(CancellationToken cancellationToken = default)
        => _context.Beans.CountAsync(cancellationToken);

    public async Task<IBeanDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await GetBeansWithCountries()
            .Where(p => p.Id == id)
            .Select(_beanSelector)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException($"Bean with id {id} was not found");

    public async Task<Bean> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Beans
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Bean with id {id} was not found");

    public async Task DeleteAsync(Bean bean, CancellationToken cancellationToken = default)
    {
        _context.Beans.Remove(bean);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(ICreateBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default)
    {
        var bean = new Bean()
        {
            Id = Guid.NewGuid(),
            Index = beanDTO.Index,
            IsBOTD = beanDTO.IsBOTD,
            Cost = beanDTO.Cost,
            ImageName = beanDTO.ImageName,
            Colour = beanDTO.Colour,
            Name = beanDTO.Name,
            Description = beanDTO.Description.Trim(),
            CountryId = countryId
        };
        _context.Beans.Add(bean);
        await _context.SaveChangesAsync(cancellationToken);
        return bean.Id;
    }
}

