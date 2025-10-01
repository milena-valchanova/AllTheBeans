using AllTheBeans.Domain.DataModels;
using System.ComponentModel.DataAnnotations;

namespace AllTheBeans.API.DataModels;

public class GetAllParameters : IGetAllParameters
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
}
