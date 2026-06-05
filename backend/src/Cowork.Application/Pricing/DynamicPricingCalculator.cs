using Cowork.Domain.Entities;

namespace Cowork.Application.Pricing;

public sealed class DynamicPricingCalculator
{
    public PricingCalculationResult Calculate(
        Space space,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        DateTimeOffset createdAt)
    {
        if (space is null)
            throw new ArgumentNullException(nameof(space));

        if (startTime >= endTime)
            throw new ArgumentException("Start time must be earlier than end time.");

        var duration = endTime - startTime;

        if (duration.TotalMinutes < 30)
            throw new ArgumentException("Reservation duration must be at least 30 minutes.");

        if (duration.TotalHours > 8)
            throw new ArgumentException("Reservation duration cannot be greater than 8 hours.");

        var baseAmount = RoundMoney(space.BaseHourlyRate * (decimal)duration.TotalHours);
        var currentAmount = baseAmount;

        var adjustments = new List<PricingAdjustment>();

        // Order:
        // 1. Peak hour surcharge
        // 2. Weekend surcharge
        // 3. Long reservation discount
        // 4. Advance booking discount

        if (IsPeakHour(startTime, endTime))
        {
            currentAmount = ApplyAdjustment(
                adjustments,
                currentAmount,
                "PeakHour",
                0.25m,
                "Reservation overlaps 09:00-11:00 or 17:00-19:00.");
        }

        if (IsWeekend(startTime))
        {
            currentAmount = ApplyAdjustment(
                adjustments,
                currentAmount,
                "Weekend",
                0.15m,
                "Reservation is on Saturday or Sunday.");
        }

        if (duration.TotalHours >= 4)
        {
            currentAmount = ApplyAdjustment(
                adjustments,
                currentAmount,
                "LongReservation",
                -0.10m,
                "Reservation duration is greater than or equal to 4 hours.");
        }

        if ((startTime - createdAt).TotalDays >= 7)
        {
            currentAmount = ApplyAdjustment(
                adjustments,
                currentAmount,
                "AdvanceBooking",
                -0.05m,
                "Reservation was created at least 7 days in advance.");
        }

        return new PricingCalculationResult(
            BaseAmount: baseAmount,
            FinalAmount: RoundMoney(currentAmount),
            Adjustments: adjustments);
    }

    private static decimal ApplyAdjustment(
        List<PricingAdjustment> adjustments,
        decimal currentAmount,
        string rule,
        decimal percentage,
        string description)
    {
        var amountBefore = RoundMoney(currentAmount);
        var amountAfter = RoundMoney(currentAmount + (currentAmount * percentage));

        adjustments.Add(new PricingAdjustment(
            Rule: rule,
            Percentage: percentage,
            Description: description,
            AmountBefore: amountBefore,
            AmountAfter: amountAfter));

        return amountAfter;
    }

    private static bool IsWeekend(DateTimeOffset startTime)
    {
        return startTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }

    private static bool IsPeakHour(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var start = TimeOnly.FromTimeSpan(startTime.TimeOfDay);
        var end = TimeOnly.FromTimeSpan(endTime.TimeOfDay);

        return Overlaps(start, end, new TimeOnly(9, 0), new TimeOnly(11, 0))
            || Overlaps(start, end, new TimeOnly(17, 0), new TimeOnly(19, 0));
    }

    private static bool Overlaps(
        TimeOnly start,
        TimeOnly end,
        TimeOnly rangeStart,
        TimeOnly rangeEnd)
    {
        return start < rangeEnd && end > rangeStart;
    }

    private static decimal RoundMoney(decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}