using Cowork.Application.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ReportsService _reportsService;

    public ReportsController(ReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    [HttpGet]
    public async Task<ActionResult<ReportsResponse>> Get(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var result = await _reportsService.GetAsync(
            new ReportsRequest(from, to),
            cancellationToken);

        return Ok(result);
    }
}