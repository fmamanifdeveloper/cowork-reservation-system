using Cowork.Application.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _reservationService.ListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _reservationService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}