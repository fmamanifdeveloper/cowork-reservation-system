using Cowork.Application.Public;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[ApiController]
[Route("api/public/spaces")]
public sealed class PublicAvailabilityController : ControllerBase
{
    private readonly PublicAvailabilityService _availabilityService;

    public PublicAvailabilityController(PublicAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpGet("{spaceId:guid}/availability")]
    public async Task<ActionResult<PublicAvailabilityResponse>> GetAvailability(
        Guid spaceId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var response = await _availabilityService.GetAvailabilityAsync(
            spaceId,
            date,
            cancellationToken);

        return Ok(response);
    }
}