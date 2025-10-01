namespace AllTheBeans.Domain.DataModels;
internal class GetSingleResultParameters : IGetAllParameters
{
    public int PageNumber => 1;

    public int PageSize => 1;

    public string? Search => null;
}
