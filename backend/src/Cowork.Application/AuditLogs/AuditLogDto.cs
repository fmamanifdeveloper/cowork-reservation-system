namespace Cowork.Application.AuditLogs;

public sealed record AuditLogDto(
    Guid Id,
    string EventType,
    string EntityType,
    Guid? EntityId,
    Guid? ActorUserId,
    Guid? ActorCustomerId,
    string Action,
    string Description,
    string? OldValues,
    string? NewValues,
    string? Metadata,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt);