using Cowork.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[Authorize(Roles = "Admin,Staff")]
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
    public async Task<ActionResult<ReportsDashboardDto>> GetDashboard(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var result = await _reportsService.GetDashboardAsync(
            from,
            to,
            cancellationToken);

        return Ok(result);
    }
}