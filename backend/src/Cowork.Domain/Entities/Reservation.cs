using Cowork.Domain.Enums;

namespace Cowork.Domain.Entities;

public sealed class Reservation
{
    public Guid Id { get; private set; }
    public Guid SpaceId { get; private set; }

    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }

    public ReservationStatus Status { get; private set; }

    public decimal BaseAmount { get; private set; }
    public decimal FinalAmount { get; private set; }
    public decimal? RefundAmount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private Reservation()
    {
    }

    public Reservation(
        Guid id,
        Guid spaceId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        decimal baseAmount,
        decimal finalAmount,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Reservation id is required.", nameof(id));

        if (spaceId == Guid.Empty)
            throw new ArgumentException("Space id is required.", nameof(spaceId));

        if (startTime >= endTime)
            throw new ArgumentException("Start time must be earlier than end time.");

        var duration = endTime - startTime;

        if (duration.TotalMinutes < 30)
            throw new ArgumentException("Reservation duration must be at least 30 minutes.");

        if (duration.TotalHours > 8)
            throw new ArgumentException("Reservation duration cannot be greater than 8 hours.");

        if (baseAmount < 0)
            throw new ArgumentException("Base amount cannot be negative.", nameof(baseAmount));

        if (finalAmount < 0)
            throw new ArgumentException("Final amount cannot be negative.", nameof(finalAmount));

        Id = id;
        SpaceId = spaceId;
        StartTime = startTime;
        EndTime = endTime;
        BaseAmount = baseAmount;
        FinalAmount = finalAmount;
        CreatedAt = createdAt;
        Status = ReservationStatus.Confirmed;
    }

    public double DurationInHours => (EndTime - StartTime).TotalHours;

    public void Cancel(decimal refundAmount, DateTimeOffset cancelledAt)
    {
        if (Status == ReservationStatus.Completed)
            throw new InvalidOperationException("Completed reservations cannot be cancelled.");

        if (Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("Reservation is already cancelled.");

        if (refundAmount < 0)
            throw new ArgumentException("Refund amount cannot be negative.", nameof(refundAmount));

        Status = ReservationStatus.Cancelled;
        RefundAmount = refundAmount;
        CancelledAt = cancelledAt;
    }

    public void Complete(DateTimeOffset completedAt)
    {
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("Cancelled reservations cannot be completed.");

        Status = ReservationStatus.Completed;
        CompletedAt = completedAt;
    }
}