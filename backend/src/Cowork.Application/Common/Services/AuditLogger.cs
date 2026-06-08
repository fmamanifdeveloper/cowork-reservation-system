using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;
using System.Text.Json;

namespace Cowork.Application.Common.Services;

public sealed class AuditLogger : IAuditLogger
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogger(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public Task LogAsync(
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
        CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog(
            Guid.NewGuid(),
            eventType,
            entityType,
            entityId,
            actorUserId,
            actorCustomerId,
            action,
            description,
            SerializeOrNull(oldValues),
            SerializeOrNull(newValues),
            SerializeOrNull(metadata),
            null,
            null,
            DateTimeOffset.UtcNow);

        _auditLogRepository.Add(auditLog);

        return Task.CompletedTask;
    }

    private static string? SerializeOrNull(object? value)
    {
        return value is null
            ? null
            : JsonSerializer.Serialize(value, JsonOptions);
    }
}