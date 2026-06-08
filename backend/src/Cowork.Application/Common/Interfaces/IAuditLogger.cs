namespace Cowork.Application.Common.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(
        string eventType,
        string entityType,
        Guid? entityId,
        Guid? actorUserId,
        Guid? actorCustomerId,
        string action,
        string description,
        object? oldValues,
        object? newValues,
        object? metadata,
        CancellationToken cancellationToken);
}