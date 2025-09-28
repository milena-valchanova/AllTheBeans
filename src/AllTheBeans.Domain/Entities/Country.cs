using System.ComponentModel.DataAnnotations;

namespace AllTheBeans.Domain.Entities;
internal class Country
{
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
