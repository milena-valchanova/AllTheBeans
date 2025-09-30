using AllTheBeans.API.DataModels;
using AllTheBeans.Domain.DataModels;

namespace AllTheBeans.API.Mappers;

public interface IBeansMapper
{
    BeanResponse ToBeanResponse(IBeanDTO beanDTO);
}
