using Cowork.Application.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/public/reservations")]
public sealed class PublicReservationsController : ControllerBase
{
    private readonly PublicReservationService _publicReservationService;

    public PublicReservationsController(PublicReservationService publicReservationService)
    {
        _publicReservationService = publicReservationService;
    }

    [HttpPost]
    public async Task<ActionResult<PublicReservationResponse>> Create(
        PublicCreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _publicReservationService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}