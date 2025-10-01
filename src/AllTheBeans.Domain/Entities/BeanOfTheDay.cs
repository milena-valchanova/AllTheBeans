using AllTheBeans.Domain.Exceptions;

namespace AllTheBeans.Domain.Entities;
public class BeanOfTheDay
{
    public Guid Id { get; set; }

    public Guid BeanId { get; set; }

    private Bean? _bean;
    public Bean Bean
    {
        get => _bean
            ?? throw new PropertyNotInitialisedException(nameof(Bean));
        set => _bean = value;
    }

    public DateOnly Date { get; set; }
}
