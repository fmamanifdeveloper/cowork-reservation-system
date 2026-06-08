using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;

namespace Cowork.Application.AuditLogs;

public sealed class AuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IReadOnlyList<AuditLogDto>> ListAsync(CancellationToken cancellationToken)
    {
        var auditLogs = await _auditLogRepository.ListAsync(cancellationToken);
        return auditLogs.Select(ToDto).ToList();
    }

    private static AuditLogDto ToDto(AuditLog auditLog)
    {
        return new AuditLogDto(
            auditLog.Id,
            auditLog.EventType,
            auditLog.EntityType,
            auditLog.EntityId,
            auditLog.ActorUserId,
            auditLog.ActorCustomerId,
            auditLog.Action,
            auditLog.Description,
            auditLog.OldValues,
            auditLog.NewValues,
            auditLog.Metadata,
            auditLog.IpAddress,
            auditLog.UserAgent,
            auditLog.CreatedAt);
    }
}