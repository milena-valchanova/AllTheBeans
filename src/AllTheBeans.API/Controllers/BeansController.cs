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
    [ProducesResponseType<BeansResponse>(StatusCodes.Status200OK, "application/json")]
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
    [ProducesResponseType<BeanResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeanOfTheDay(CancellationToken cancellationToken)
    {
        var currentDay = DateOnly.FromDateTime(DateTime.UtcNow);
        var bean = await _beansService.GetOrCreateBeanOfTheDayAsync(currentDay, cancellationToken);
        var result = _beansMapper.ToBeanResponse(bean);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<BeanResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var bean = await _beansService.GetByIdAsync(id, cancellationToken);
        var result = _beansMapper.ToBeanResponse(bean);
        return Ok(result);
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<BeanResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrUpdateBeanPayload payload, 
        CancellationToken cancellationToken)
    {
        var bean = await _beansService.CreateAsync(payload, cancellationToken);
        var result = _beansMapper.ToBeanResponse(bean);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType<BeanResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrUpdate(
        Guid id,
        [FromBody] CreateOrUpdateBeanPayload payload,
        CancellationToken cancellationToken)
    {
        var bean = await _beansService.CreateOrUpdateAsync(id, payload, cancellationToken);
        var result = _beansMapper.ToBeanResponse(bean);
        return Ok(result);
    }

    [HttpPatch("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType<BeanResponse>(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateBeanPayload payload,
        CancellationToken cancellationToken)
    {
        await _beansService.UpdateAsync(id, payload, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType<BeanResponse>(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _beansService.DeleteBeanAsync(id, cancellationToken);
        return NoContent();
    }
}
