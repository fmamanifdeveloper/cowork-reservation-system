using Cowork.Application.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[Authorize(Roles = "Admin,Staff,Customer")]
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
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> List(
        CancellationToken cancellationToken)
    {
        var result = await _reservationService.ListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReservationDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _reservationService.GetByIdAsync(id, cancellationToken);
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

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ReservationDto>> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _reservationService.CancelAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ReservationDto>> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _reservationService.CompleteAsync(id, cancellationToken);
        return Ok(result);
    }
}