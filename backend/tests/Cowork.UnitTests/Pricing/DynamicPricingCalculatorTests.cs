using Cowork.Application.Pricing;
using Cowork.Domain.Entities;
using Cowork.Domain.Enums;
using FluentAssertions;

namespace Cowork.UnitTests.Pricing;

public sealed class DynamicPricingCalculatorTests
{
    private readonly DynamicPricingCalculator _calculator = new();

    [Fact]
    public void Calculate_ShouldReturnBaseAmount_WhenNoRulesApply()
    {
        var space = CreateSpace(baseHourlyRate: 50m);

        var startTime = new DateTimeOffset(2026, 6, 8, 13, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 6, 8, 15, 0, 0, TimeSpan.FromHours(-5));
        var createdAt = startTime;

        var result = _calculator.Calculate(space, startTime, endTime, createdAt);

        result.BaseAmount.Should().Be(100m);
        result.FinalAmount.Should().Be(100m);
        result.Adjustments.Should().BeEmpty();
    }

    [Fact]
    public void Calculate_ShouldApplyPeakHourSurcharge_WhenReservationOverlapsPeakHour()
    {
        var space = CreateSpace(baseHourlyRate: 50m);

        var startTime = new DateTimeOffset(2026, 6, 8, 9, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.FromHours(-5));
        var createdAt = startTime;

        var result = _calculator.Calculate(space, startTime, endTime, createdAt);

        result.BaseAmount.Should().Be(50m);
        result.FinalAmount.Should().Be(62.50m);

        result.Adjustments.Should().ContainSingle();
        result.Adjustments[0].Rule.Should().Be("PeakHour");
        result.Adjustments[0].Percentage.Should().Be(0.25m);
        result.Adjustments[0].AmountBefore.Should().Be(50m);
        result.Adjustments[0].AmountAfter.Should().Be(62.50m);
    }

    [Fact]
    public void Calculate_ShouldApplyWeekendSurcharge_WhenReservationIsOnWeekend()
    {
        var space = CreateSpace(baseHourlyRate: 50m);

        var startTime = new DateTimeOffset(2026, 6, 13, 13, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 6, 13, 15, 0, 0, TimeSpan.FromHours(-5));
        var createdAt = startTime;

        var result = _calculator.Calculate(space, startTime, endTime, createdAt);

        result.BaseAmount.Should().Be(100m);
        result.FinalAmount.Should().Be(115m);

        result.Adjustments.Should().ContainSingle();
        result.Adjustments[0].Rule.Should().Be("Weekend");
        result.Adjustments[0].Percentage.Should().Be(0.15m);
        result.Adjustments[0].AmountBefore.Should().Be(100m);
        result.Adjustments[0].AmountAfter.Should().Be(115m);
    }

    [Fact]
    public void Calculate_ShouldApplyLongReservationDiscount_WhenDurationIsAtLeastFourHours()
    {
        var space = CreateSpace(baseHourlyRate: 50m);

        var startTime = new DateTimeOffset(2026, 6, 8, 13, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 6, 8, 17, 0, 0, TimeSpan.FromHours(-5));
        var createdAt = startTime;

        var result = _calculator.Calculate(space, startTime, endTime, createdAt);

        result.BaseAmount.Should().Be(200m);
        result.FinalAmount.Should().Be(180m);

        result.Adjustments.Should().ContainSingle();
        result.Adjustments[0].Rule.Should().Be("LongReservation");
        result.Adjustments[0].Percentage.Should().Be(-0.10m);
        result.Adjustments[0].AmountBefore.Should().Be(200m);
        result.Adjustments[0].AmountAfter.Should().Be(180m);
    }

    [Fact]
    public void Calculate_ShouldApplyAdvanceBookingDiscount_WhenCreatedAtLeastSevenDaysBeforeStart()
    {
        var space = CreateSpace(baseHourlyRate: 50m);

        var startTime = new DateTimeOffset(2026, 6, 8, 13, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 6, 8, 15, 0, 0, TimeSpan.FromHours(-5));
        var createdAt = startTime.AddDays(-8);

        var result = _calculator.Calculate(space, startTime, endTime, createdAt);

        result.BaseAmount.Should().Be(100m);
        result.FinalAmount.Should().Be(95m);

        result.Adjustments.Should().ContainSingle();
        result.Adjustments[0].Rule.Should().Be("AdvanceBooking");
        result.Adjustments[0].Percentage.Should().Be(-0.05m);
        result.Adjustments[0].AmountBefore.Should().Be(100m);
        result.Adjustments[0].AmountAfter.Should().Be(95m);
    }

    [Fact]
    public void Calculate_ShouldApplyRulesInExpectedOrder_WhenMultipleRulesApply()
    {
        var space = CreateSpace(baseHourlyRate: 50m);

        var startTime = new DateTimeOffset(2026, 6, 13, 9, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 6, 13, 13, 0, 0, TimeSpan.FromHours(-5));
        var createdAt = startTime.AddDays(-8);

        var result = _calculator.Calculate(space, startTime, endTime, createdAt);

        result.BaseAmount.Should().Be(200m);
        result.FinalAmount.Should().Be(245.81m);

        result.Adjustments.Should().HaveCount(4);
        result.Adjustments[0].Rule.Should().Be("PeakHour");
        result.Adjustments[1].Rule.Should().Be("Weekend");
        result.Adjustments[2].Rule.Should().Be("LongReservation");
        result.Adjustments[3].Rule.Should().Be("AdvanceBooking");

        result.Adjustments[0].AmountBefore.Should().Be(200m);
        result.Adjustments[0].AmountAfter.Should().Be(250m);

        result.Adjustments[1].AmountBefore.Should().Be(250m);
        result.Adjustments[1].AmountAfter.Should().Be(287.50m);

        result.Adjustments[2].AmountBefore.Should().Be(287.50m);
        result.Adjustments[2].AmountAfter.Should().Be(258.75m);

        result.Adjustments[3].AmountBefore.Should().Be(258.75m);
        result.Adjustments[3].AmountAfter.Should().Be(245.81m);
    }

    [Fact]
    public void Calculate_ShouldThrow_WhenStartTimeIsGreaterThanOrEqualToEndTime()
    {
        var space = CreateSpace(baseHourlyRate: 50m);

        var startTime = new DateTimeOffset(2026, 6, 8, 15, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 6, 8, 15, 0, 0, TimeSpan.FromHours(-5));
        var createdAt = startTime;

        var action = () => _calculator.Calculate(space, startTime, endTime, createdAt);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Start time must be earlier than end time.");
    }

    [Fact]
    public void Calculate_ShouldThrow_WhenDurationIsLessThanThirtyMinutes()
    {
        var space = CreateSpace(baseHourlyRate: 50m);

        var startTime = new DateTimeOffset(2026, 6, 8, 13, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 6, 8, 13, 15, 0, TimeSpan.FromHours(-5));
        var createdAt = startTime;

        var action = () => _calculator.Calculate(space, startTime, endTime, createdAt);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Reservation duration must be at least 30 minutes.");
    }

    [Fact]
    public void Calculate_ShouldThrow_WhenDurationIsGreaterThanEightHours()
    {
        var space = CreateSpace(baseHourlyRate: 50m);

        var startTime = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 6, 8, 17, 0, 0, TimeSpan.FromHours(-5));
        var createdAt = startTime;

        var action = () => _calculator.Calculate(space, startTime, endTime, createdAt);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Reservation duration cannot be greater than 8 hours.");
    }

    private static Space CreateSpace(decimal baseHourlyRate)
    {
        return new Space(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "Sala de pruebas",
            10,
            baseHourlyRate,
            new TimeOnly(8, 0),
            new TimeOnly(22, 0),
            SpaceStatus.Active,
            "America/Lima",
            Guid.Parse("00000000-0000-0000-0000-000000000101"));
    }
}