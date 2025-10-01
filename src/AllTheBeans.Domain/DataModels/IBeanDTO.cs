namespace AllTheBeans.Domain.DataModels;
public interface IBeanDTO : ICreateOrUpdateBeanDTO
{
    Guid Id { get; }
}
