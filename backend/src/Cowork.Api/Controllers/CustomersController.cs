using Cowork.Application.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[Authorize(Roles = "Admin,Staff")]
[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;

    public CustomersController(CustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _customerService.ListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _customerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}