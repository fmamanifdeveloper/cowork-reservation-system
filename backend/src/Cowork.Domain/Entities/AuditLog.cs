namespace Cowork.Domain.Entities;

public sealed class AuditLog
{
    private AuditLog()
    {
        EventType = string.Empty;
        EntityType = string.Empty;
        Action = string.Empty;
        Description = string.Empty;
    }

    public AuditLog(
        Guid id,
        string eventType,
        string entityType,
        Guid? entityId,
        Guid? actorUserId,
        Guid? actorCustomerId,
        string action,
        string description,
        string? oldValues,
        string? newValues,
        string? metadata,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action is required.", nameof(action));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        Id = id;
        EventType = eventType.Trim();
        EntityType = entityType.Trim();
        EntityId = entityId;
        ActorUserId = actorUserId;
        ActorCustomerId = actorCustomerId;
        Action = action.Trim();
        Description = description.Trim();
        OldValues = oldValues;
        NewValues = newValues;
        Metadata = metadata;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string EventType { get; private set; }
    public string EntityType { get; private set; }
    public Guid? EntityId { get; private set; }

    public Guid? ActorUserId { get; private set; }
    public Guid? ActorCustomerId { get; private set; }

    public string Action { get; private set; }
    public string Description { get; private set; }

    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? Metadata { get; private set; }

    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}