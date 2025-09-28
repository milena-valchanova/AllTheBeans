using AllTheBeans.Domain.Entities.Constants;
using AllTheBeans.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace AllTheBeans.Domain.Entities;
public class Country
{
    public long Id { get; set; }

    [Required]
    [StringLength(PropertyLengths.Name)]
    public string Name { get; set; } = string.Empty;

    private HashSet<Bean>? _beans;
    public HashSet<Bean> Beans
    {
        get => _beans
            ?? throw new PropertyNotInitialisedException(nameof(Beans));
        set => _beans = value;
    }
}
