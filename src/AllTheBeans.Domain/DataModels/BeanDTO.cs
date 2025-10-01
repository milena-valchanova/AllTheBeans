namespace AllTheBeans.Domain.DataModels;
internal class BeanDTO : CreateOrUpdateBeanDTO, IBeanDTO
{
    public Guid Id { get; set; }
}
