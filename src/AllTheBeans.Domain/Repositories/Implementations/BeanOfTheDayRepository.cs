using AllTheBeans.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AllTheBeans.Domain.Repositories.Implementations;
internal class BeanOfTheDayRepository(BeansContext _context) : IBeanOfTheDayRepository
{
    public Task<BeanOfTheDay?> GetOrDefaultAsync(DateOnly date, CancellationToken cancellationToken = default)
        => _context.BeansOfTheDay
            .FirstOrDefaultAsync(p => p.Date == date, cancellationToken);

    public async Task<BeanOfTheDay> CreateAsync(Guid beanId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var beanOfTheDay = new BeanOfTheDay()
        {
            BeanId = beanId,
            Date = date
        };
        _context.BeansOfTheDay.Add(beanOfTheDay);
        await _context.SaveChangesAsync(cancellationToken);
        return beanOfTheDay;
    }
}
