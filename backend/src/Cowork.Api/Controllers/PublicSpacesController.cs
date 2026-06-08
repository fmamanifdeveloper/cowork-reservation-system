using Cowork.Application.Public;
using Cowork.Application.Spaces;
using Cowork.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/public/spaces")]
public sealed class PublicSpacesController : ControllerBase
{
    private readonly SpaceService _spaceService;

    public PublicSpacesController(SpaceService spaceService)
    {
        _spaceService = spaceService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PublicSpaceDto>>> List(
        CancellationToken cancellationToken)
    {
        var spaces = await _spaceService.ListAsync(cancellationToken);

        var result = spaces
            .Where(x => x.Status == SpaceStatus.Active)
            .Select(x => new PublicSpaceDto(
                x.Id,
                x.Name,
                x.Capacity,
                x.BaseHourlyRate,
                x.OpeningTime,
                x.ClosingTime,
                x.TimeZoneId,
                x.Status))
            .ToList();

        return Ok(result);
    }
}