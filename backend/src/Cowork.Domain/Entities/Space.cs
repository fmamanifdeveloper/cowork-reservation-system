using Cowork.Domain.Enums;

namespace Cowork.Domain.Entities;

public sealed class Space
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public int Capacity { get; private set; }
    public decimal BaseHourlyRate { get; private set; }
    public TimeOnly OpeningTime { get; private set; }
    public TimeOnly ClosingTime { get; private set; }
    public SpaceStatus Status { get; private set; }

    private Space()
    {
    }

    public Space(
        Guid id,
        string name,
        int capacity,
        decimal baseHourlyRate,
        TimeOnly openingTime,
        TimeOnly closingTime,
        SpaceStatus status)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Space id is required.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Space name is required.", nameof(name));

        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

        if (baseHourlyRate <= 0)
            throw new ArgumentException("Base hourly rate must be greater than zero.", nameof(baseHourlyRate));

        if (openingTime >= closingTime)
            throw new ArgumentException("Opening time must be earlier than closing time.");

        Id = id;
        Name = name.Trim();
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        Status = status;
    }

    public bool IsAvailableForReservation()
    {
        return Status == SpaceStatus.Active;
    }

    public void SetMaintenance()
    {
        Status = SpaceStatus.Maintenance;
    }

    public void Activate()
    {
        Status = SpaceStatus.Active;
    }
}