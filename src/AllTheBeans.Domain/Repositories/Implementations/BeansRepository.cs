using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;

namespace AllTheBeans.Domain.Repositories.Implementations;
internal class BeansRepository(BeansContext _context) : IBeansRepository
{
    public async Task CreateAsync(IBeanDTO beanDTO, long countryId, CancellationToken cancellationToken = default)
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
            Description = beanDTO.Description,
            CountryId = countryId
        };
        _context.Beans.Add(bean);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

