namespace AllTheBeans.Domain.DataModels;
internal class BeanDTO : CreateBeanDTO, IBeanDTO
{
    public Guid Id { get; set; }
}
