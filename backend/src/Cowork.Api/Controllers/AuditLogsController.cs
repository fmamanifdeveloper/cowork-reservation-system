using Cowork.Application.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[Authorize(Roles = "Admin,Staff")]
[ApiController]
[Route("api/audit-logs")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly AuditLogService _auditLogService;

    public AuditLogsController(AuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> List(
        CancellationToken cancellationToken)
    {
        var result = await _auditLogService.ListAsync(cancellationToken);
        return Ok(result);
    }
}