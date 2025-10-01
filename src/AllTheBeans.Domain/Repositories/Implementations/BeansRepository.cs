using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Enums;
using EnumsNET;
using Microsoft.EntityFrameworkCore;

namespace AllTheBeans.Domain.Repositories.Implementations;
internal class BeansRepository(BeansContext _context) : IBeansRepository
{
    public IQueryable<Bean> GetAll(IGetAllParameters getAllParameters, CancellationToken cancellationToken = default)
    {
        if (getAllParameters.PageNumber <= 0)
            throw new ArgumentException($"{nameof(getAllParameters.PageNumber)} must have positive value");
        if (getAllParameters.PageSize <= 0)
            throw new ArgumentException($"{nameof(getAllParameters.PageSize)} must have positive value");

        var entitiesToSkip = (getAllParameters.PageNumber - 1) * getAllParameters.PageSize;
        
        return Search(getAllParameters)
            .OrderBy(p => p.Id)
            .Skip(entitiesToSkip)
            .Take(getAllParameters.PageSize);
    }

    public Task<int> CountAllAsync(ISearchParameters searchParameters, CancellationToken cancellationToken = default)
        => Search(searchParameters).CountAsync(cancellationToken);

    private IQueryable<Bean> Search(ISearchParameters searchParameters)
    {
        var query = _context.Beans
            .Include(p => p.Country)
            .AsNoTracking();

        if (string.IsNullOrWhiteSpace(searchParameters.Search))
        {
            return query;            
        }
        var search = searchParameters.Search.Trim().ToLower();
        var matchingColours = Enum.GetValues<BeanColour>()
            .Where(p => {
                var description = p.AsString(EnumFormat.Description);
                if (description is null)
                    return false;
                return description.ToLower().Contains(search);
            })
            .ToList();

        return query
            .Where(p =>
                p.Name.ToLower().Contains(search)
                || p.Description.ToLower().Contains(search)
                || p.Country.Name.ToLower().Contains(search)
                || matchingColours.Contains(p.Colour)
            );
    }

    public async Task<Bean> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await GetByIdOrDefaultAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Bean with id {id} was not found");

    public Task<Bean?> GetByIdOrDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    => _context.Beans
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task DeleteAsync(Bean bean, CancellationToken cancellationToken = default)
    {
        _context.Beans.Remove(bean);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(ICreateOrUpdateBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default)
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

    public async Task UpdateAsync(Bean bean, ICreateOrUpdateBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default)
    {
        bean.Colour = beanDTO.Colour;
        bean.Name = beanDTO.Name;
        bean.Description = beanDTO.Description;
        bean.Cost = beanDTO.Cost;
        bean.Index = beanDTO.Index;
        bean.ImageName = beanDTO.ImageName;
        bean.IsBOTD = beanDTO.IsBOTD;
        bean.CountryId = countryId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

