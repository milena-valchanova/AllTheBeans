using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Enums;
using EnumsNET;
using Microsoft.EntityFrameworkCore;

namespace AllTheBeans.Domain.Repositories.Implementations;
internal class BeansRepository(BeansContext _context) : IBeansRepository
{
    public IQueryable<Bean> GetAll(IGetAllParameters getAllParameters)
    {
        if (getAllParameters.PageNumber is null && getAllParameters.PageSize is null)
        {
            return Search(getAllParameters);
        }

        if (getAllParameters.PageNumber is null || getAllParameters.PageSize is null)
        {
            throw new ArgumentException($"Both {nameof(getAllParameters.PageNumber)} and {nameof(getAllParameters.PageSize)} "
                + "must be provided or none of them");
        }
        if (getAllParameters.PageNumber <= 0)
            throw new ArgumentException($"{nameof(getAllParameters.PageNumber)} must have positive value");
        if (getAllParameters.PageSize <= 0)
            throw new ArgumentException($"{nameof(getAllParameters.PageSize)} must have positive value");

        var entitiesToSkip = (getAllParameters.PageNumber.Value - 1) * getAllParameters.PageSize.Value;
        
        return Search(getAllParameters)
            .OrderBy(p => p.Id)
            .Skip(entitiesToSkip)
            .Take(getAllParameters.PageSize.Value);
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

    public async Task UpdateAsync(Guid beanId, IUpdateBeanDTO beanDTO, long? countryId, CancellationToken cancellationToken = default)
    {
        var bean = await _context.Beans.FindAsync(beanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bean with id {beanId} was not found");

        if (beanDTO.Colour is not null)
            bean.Colour = beanDTO.Colour.Value;
        if (beanDTO.Name is not null)
            bean.Name = beanDTO.Name;
        if (beanDTO.Description is not null)
            bean.Description = beanDTO.Description;
        if (beanDTO.Cost is not null)
            bean.Cost = beanDTO.Cost.Value;
        if (beanDTO.Index is not null)
            bean.Index = beanDTO.Index.Value;
        if (beanDTO.ImageName is not null)
            bean.ImageName = beanDTO.ImageName;
        if (beanDTO.IsBOTD is not null)
            bean.IsBOTD = beanDTO.IsBOTD.Value;
        if (countryId is not null)
            bean.CountryId = countryId.Value;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

