using AllTheBeans.API.DataModels;
using AllTheBeans.API.Mappers;
using AllTheBeans.Domain.Repositories;
using AllTheBeans.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace AllTheBeans.API.Controllers;

[ApiController]
[Route("[controller]")]
public class BeansController(
    IBeansRepository _beansRepository,
    IBeansMapper _beansMapper,
    IBeansService _beansService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParameters paginationParameters,
        CancellationToken cancellationToken)
    {
        var beans = await _beansRepository.GetAllAsync(
            paginationParameters.PageNumber,
            paginationParameters.PageSize,
            cancellationToken);
        var totalBeans = await _beansRepository.CountAllAsync(cancellationToken);

        var result = new BeansResponse()
        {
            Beans = [.. beans.Select(_beansMapper.ToBeanResponse)],
            Total = totalBeans
        };
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var bean = await _beansRepository.GetByIdAsync(id, cancellationToken);
        var result = _beansMapper.ToBeanResponse(bean);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBeanPayload payload, 
        CancellationToken cancellationToken)
    {
        var beanId = await _beansService.InitiliseAsync(payload, cancellationToken);
        return Ok(beanId);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _beansService.DeleteBeanAsync(id, cancellationToken);
        return NoContent();
    }
}
