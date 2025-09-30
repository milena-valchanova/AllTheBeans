namespace AllTheBeans.Domain.DataModels;
public interface IBeanDTO : ICreateBeanDTO
{
    Guid Id { get; }
}
