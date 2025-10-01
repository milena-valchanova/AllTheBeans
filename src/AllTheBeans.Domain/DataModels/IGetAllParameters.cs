namespace AllTheBeans.Domain.DataModels;
public interface IGetAllParameters : ISearchParameters
{
    int? PageNumber { get; }
    int? PageSize { get; }
}
