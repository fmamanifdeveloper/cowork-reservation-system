using Cowork.Application.Pricing;
using Cowork.Domain.Entities;
using Cowork.Domain.Enums;

namespace Cowork.UnitTests.Pricing;

public sealed class DynamicPricingCalculatorTests
{
    private readonly DynamicPricingCalculator _calculator = new();

    [Fact]
    public void Calculate_ShouldReturnBaseAmount_WhenNoPricingRulesApply()
    {
        var space = CreateSpace(baseHourlyRate: 100m);

        var createdAt = new DateTimeOffset(2026, 7, 14, 8, 0, 0, TimeSpan.FromHours(-5));
        var startTime = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 7, 15, 14, 0, 0, TimeSpan.FromHours(-5));

        var result = _calculator.Calculate(
            space,
            startTime,
            endTime,
            createdAt);

        Assert.Equal(200m, result.BaseAmount);
        Assert.Equal(200m, result.FinalAmount);
    }

    [Fact]
    public void Calculate_ShouldApplyPeakHourSurcharge_WhenReservationOverlapsPeakHour()
    {
        var space = CreateSpace(baseHourlyRate: 100m);

        var createdAt = new DateTimeOffset(2026, 7, 14, 8, 0, 0, TimeSpan.FromHours(-5));
        var startTime = new DateTimeOffset(2026, 7, 15, 10, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 7, 15, 11, 0, 0, TimeSpan.FromHours(-5));

        var result = _calculator.Calculate(
            space,
            startTime,
            endTime,
            createdAt);

        Assert.Equal(100m, result.BaseAmount);
        Assert.Equal(125m, result.FinalAmount);
    }

    [Fact]
    public void Calculate_ShouldApplyWeekendSurcharge_WhenReservationIsOnWeekend()
    {
        var space = CreateSpace(baseHourlyRate: 100m);

        var createdAt = new DateTimeOffset(2026, 7, 17, 8, 0, 0, TimeSpan.FromHours(-5));
        var startTime = new DateTimeOffset(2026, 7, 18, 12, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 7, 18, 14, 0, 0, TimeSpan.FromHours(-5));

        var result = _calculator.Calculate(
            space,
            startTime,
            endTime,
            createdAt);

        Assert.Equal(200m, result.BaseAmount);
        Assert.Equal(230m, result.FinalAmount);
    }

    [Fact]
    public void Calculate_ShouldApplyLongReservationDiscount_WhenDurationIsAtLeastFourHours()
    {
        var space = CreateSpace(baseHourlyRate: 100m);

        var createdAt = new DateTimeOffset(2026, 7, 14, 8, 0, 0, TimeSpan.FromHours(-5));
        var startTime = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 7, 15, 16, 0, 0, TimeSpan.FromHours(-5));

        var result = _calculator.Calculate(
            space,
            startTime,
            endTime,
            createdAt);

        Assert.Equal(400m, result.BaseAmount);
        Assert.Equal(360m, result.FinalAmount);
    }

    [Fact]
    public void Calculate_ShouldApplyEarlyBookingDiscount_WhenReservationIsCreatedAtLeastSevenDaysBeforeStart()
    {
        var space = CreateSpace(baseHourlyRate: 100m);

        var createdAt = new DateTimeOffset(2026, 7, 1, 8, 0, 0, TimeSpan.FromHours(-5));
        var startTime = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 7, 15, 14, 0, 0, TimeSpan.FromHours(-5));

        var result = _calculator.Calculate(
            space,
            startTime,
            endTime,
            createdAt);

        Assert.Equal(200m, result.BaseAmount);
        Assert.Equal(190m, result.FinalAmount);
    }

    [Fact]
    public void Calculate_ShouldApplyRulesInDocumentedOrder_WhenAllRulesApply()
    {
        var space = CreateSpace(baseHourlyRate: 100m);

        var createdAt = new DateTimeOffset(2026, 7, 1, 8, 0, 0, TimeSpan.FromHours(-5));
        var startTime = new DateTimeOffset(2026, 7, 18, 9, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2026, 7, 18, 13, 0, 0, TimeSpan.FromHours(-5));

        var result = _calculator.Calculate(
            space,
            startTime,
            endTime,
            createdAt);

        Assert.Equal(400m, result.BaseAmount);
        Assert.Equal(491.63m, result.FinalAmount);
    }

    private static Space CreateSpace(decimal baseHourlyRate)
    {
        return new Space(
            Guid.NewGuid(),
            "Sala Test",
            10,
            baseHourlyRate,
            new TimeOnly(8, 0),
            new TimeOnly(22, 0),
            SpaceStatus.Active,
            "America/Lima");
    }
}