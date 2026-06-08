using Cowork.Domain.Enums;

namespace Cowork.Domain.Entities;

public sealed class Space
{
    private Space()
    {
        Name = string.Empty;
        TimeZoneId = "America/Lima";
    }

    public Space(
        Guid id,
        string name,
        int capacity,
        decimal baseHourlyRate,
        TimeOnly openingTime,
        TimeOnly closingTime,
        SpaceStatus status,
        string timeZoneId = "America/Lima",
        Guid? createdByUserId = null)
    {
        Validate(name, capacity, baseHourlyRate, openingTime, closingTime, timeZoneId);

        Id = id;
        Name = name.Trim();
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        TimeZoneId = timeZoneId.Trim();
        Status = status;

        IsDeleted = false;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedByUserId = createdByUserId;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }
    public int Capacity { get; private set; }
    public decimal BaseHourlyRate { get; private set; }

    public TimeOnly OpeningTime { get; private set; }
    public TimeOnly ClosingTime { get; private set; }
    public string TimeZoneId { get; private set; }

    public SpaceStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public Guid? CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    public int Version { get; private set; }

    public bool IsAvailableForReservation()
    {
        return Status == SpaceStatus.Active && !IsDeleted;
    }

    public void Update(
        string name,
        int capacity,
        decimal baseHourlyRate,
        TimeOnly openingTime,
        TimeOnly closingTime,
        SpaceStatus status,
        string timeZoneId = "America/Lima",
        Guid? updatedByUserId = null)
    {
        Validate(name, capacity, baseHourlyRate, openingTime, closingTime, timeZoneId);

        Name = name.Trim();
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        TimeZoneId = timeZoneId.Trim();
        Status = status;
        UpdatedByUserId = updatedByUserId;
    }

    public void Delete(Guid? deletedByUserId = null)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByUserId = deletedByUserId;
        Status = SpaceStatus.Inactive;
    }

    private static void Validate(
        string name,
        int capacity,
        decimal baseHourlyRate,
        TimeOnly openingTime,
        TimeOnly closingTime,
        string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Space name is required.", nameof(name));

        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

        if (baseHourlyRate <= 0)
            throw new ArgumentException("Base hourly rate must be greater than zero.", nameof(baseHourlyRate));

        if (openingTime >= closingTime)
            throw new ArgumentException("Opening time must be earlier than closing time.");

        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new ArgumentException("Time zone is required.", nameof(timeZoneId));
    }
}