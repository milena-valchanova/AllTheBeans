using AllTheBeans.API.DataModels;
using AllTheBeans.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllTheBeans.API.Controllers;

[ApiController]
[Route("[controller]")]
public class BeansController(IBeansInitialisationService _beansInitialisationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBeanPayload payload, 
        CancellationToken cancellationToken)
    {
        var beanId = await _beansInitialisationService.InitiliseAsync(payload, cancellationToken);
        return Ok(beanId);
    }
}
