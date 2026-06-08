using Cowork.Application.Spaces;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[ApiController]
[Route("api/spaces")]
public sealed class SpacesController : ControllerBase
{
    private readonly SpaceService _spaceService;

    public SpacesController(SpaceService spaceService)
    {
        _spaceService = spaceService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SpaceDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _spaceService.ListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SpaceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _spaceService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SpaceDto>> Create(
        CreateSpaceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _spaceService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SpaceDto>> Update(
        Guid id,
        UpdateSpaceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _spaceService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _spaceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}