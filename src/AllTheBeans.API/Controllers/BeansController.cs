using AllTheBeans.API.DataModels;
using AllTheBeans.API.Mappers;
using AllTheBeans.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllTheBeans.API.Controllers;

[ApiController]
[Route("[controller]")]
public class BeansController(
    IBeansMapper _beansMapper,
    IBeansService _beansService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllParameters getAllParameters,
        CancellationToken cancellationToken)
    {
        var beans = await _beansService.GetAllAsync(
            getAllParameters,
            cancellationToken);
        var totalBeans = await _beansService.CountAllAsync(getAllParameters, cancellationToken);

        var result = new BeansResponse()
        {
            Beans = [.. beans.Select(_beansMapper.ToBeanResponse)],
            Total = totalBeans
        };
        return Ok(result);
    }

    [HttpGet("of-the-day")]
    public async Task<IActionResult> BeanOfTheDay(CancellationToken cancellationToken)
    {
        var currentDay = DateOnly.FromDateTime(DateTime.UtcNow);
        var bean = await _beansService.GetOrCreateBeanOfTheDayAsync(currentDay, cancellationToken);
        var result = _beansMapper.ToBeanResponse(bean);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var bean = await _beansService.GetByIdAsync(id, cancellationToken);
        var result = _beansMapper.ToBeanResponse(bean);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrUpdateBeanPayload payload, 
        CancellationToken cancellationToken)
    {
        var bean = await _beansService.CreateAsync(payload, cancellationToken);
        var result = _beansMapper.ToBeanResponse(bean);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateOrUpdateBeanPayload payload,
        CancellationToken cancellationToken)
    {
        var bean = await _beansService.CreateOrUpdateAsync(id, payload, cancellationToken);
        var result = _beansMapper.ToBeanResponse(bean);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _beansService.DeleteBeanAsync(id, cancellationToken);
        return NoContent();
    }
}
