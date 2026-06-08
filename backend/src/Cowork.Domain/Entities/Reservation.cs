using Cowork.Domain.Enums;

namespace Cowork.Domain.Entities;

public sealed class Reservation
{
    private Reservation()
    {
        ReservationCode = string.Empty;
        PricingBreakdown = "{}";
    }

    public Reservation(
        Guid id,
        string reservationCode,
        Guid spaceId,
        Guid customerId,
        Guid? createdByUserId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        decimal baseAmount,
        decimal finalAmount,
        string pricingBreakdown,
        DateTimeOffset createdAt)
    {
        ValidateSchedule(startTime, endTime);
        ValidateAmounts(baseAmount, finalAmount);

        if (string.IsNullOrWhiteSpace(reservationCode))
            throw new ArgumentException("Reservation code is required.", nameof(reservationCode));

        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        Id = id;
        ReservationCode = reservationCode.Trim();
        SpaceId = spaceId;
        CustomerId = customerId;
        CreatedByUserId = createdByUserId;

        StartTime = startTime;
        EndTime = endTime;
        Status = ReservationStatus.Confirmed;

        BaseAmount = baseAmount;
        FinalAmount = finalAmount;
        RefundAmount = null;
        PricingBreakdown = string.IsNullOrWhiteSpace(pricingBreakdown)
            ? "{}"
            : pricingBreakdown;

        CreatedAt = createdAt;
        Version = 1;
    }

    // Constructor antiguo para no romper tests o código previo.
    public Reservation(
        Guid id,
        Guid spaceId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        decimal baseAmount,
        decimal finalAmount,
        DateTimeOffset createdAt)
    {
        ValidateSchedule(startTime, endTime);
        ValidateAmounts(baseAmount, finalAmount);

        Id = id;
        ReservationCode = $"RSV-{createdAt:yyyyMMddHHmmssfff}";
        SpaceId = spaceId;
        CustomerId = Guid.Empty;
        CreatedByUserId = null;

        StartTime = startTime;
        EndTime = endTime;
        Status = ReservationStatus.Confirmed;

        BaseAmount = baseAmount;
        FinalAmount = finalAmount;
        RefundAmount = null;
        PricingBreakdown = "{}";

        CreatedAt = createdAt;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public string ReservationCode { get; private set; }

    public Guid SpaceId { get; private set; }
    public Guid CustomerId { get; private set; }

    public Guid? CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public Guid? CompletedByUserId { get; private set; }

    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }

    public ReservationStatus Status { get; private set; }

    public decimal BaseAmount { get; private set; }
    public decimal FinalAmount { get; private set; }
    public decimal? RefundAmount { get; private set; }

    public string PricingBreakdown { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public int Version { get; private set; }

    public decimal DurationInHours => (decimal)(EndTime - StartTime).TotalHours;

    public void Cancel(
        decimal refundAmount,
        DateTimeOffset cancelledAt,
        Guid? cancelledByUserId = null)
    {
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("Reservation is already cancelled.");

        if (Status == ReservationStatus.Completed)
            throw new InvalidOperationException("Completed reservations cannot be cancelled.");

        if (refundAmount < 0)
            throw new ArgumentException("Refund amount cannot be negative.", nameof(refundAmount));

        Status = ReservationStatus.Cancelled;
        RefundAmount = refundAmount;
        CancelledAt = cancelledAt;
        CancelledByUserId = cancelledByUserId;
    }

    public void Complete(DateTimeOffset completedAt, Guid? completedByUserId = null)
    {
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("Cancelled reservations cannot be completed.");

        Status = ReservationStatus.Completed;
        CompletedAt = completedAt;
        CompletedByUserId = completedByUserId;
    }

    private static void ValidateSchedule(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Reservation start time must be earlier than end time.");

        var duration = endTime - startTime;

        if (duration < TimeSpan.FromMinutes(30))
            throw new ArgumentException("Reservation duration must be at least 30 minutes.");

        if (duration > TimeSpan.FromHours(8))
            throw new ArgumentException("Reservation duration cannot exceed 8 hours.");
    }

    private static void ValidateAmounts(decimal baseAmount, decimal finalAmount)
    {
        if (baseAmount < 0)
            throw new ArgumentException("Base amount cannot be negative.", nameof(baseAmount));

        if (finalAmount < 0)
            throw new ArgumentException("Final amount cannot be negative.", nameof(finalAmount));
    }
}