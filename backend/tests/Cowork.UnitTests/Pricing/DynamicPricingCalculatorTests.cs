using Cowork.Application.Pricing;
using Cowork.Domain.Entities;
using Cowork.Domain.Enums;

namespace Cowork.UnitTests.Pricing;

public sealed class DynamicPricingCalculatorTests
{
    private readonly DynamicPricingCalculator _calculator = new();

    [Fact]
    public void Calculate_ShouldReturnBaseAmount_WhenNoRulesApply()
    {
        var space = CreateSpace(100m);

        var createdAt = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
        var start = new DateTimeOffset(2026, 6, 2, 12, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 6, 2, 13, 0, 0, TimeSpan.Zero);

        var result = _calculator.Calculate(space, start, end, createdAt);

        Assert.Equal(100m, result.BaseAmount);
        Assert.Equal(100m, result.FinalAmount);
        Assert.Empty(result.Adjustments);
    }

    [Fact]
    public void Calculate_ShouldApplyPeakHourSurcharge_WhenReservationOverlapsPeakHour()
    {
        var space = CreateSpace(100m);

        var createdAt = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
        var start = new DateTimeOffset(2026, 6, 2, 9, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 6, 2, 10, 0, 0, TimeSpan.Zero);

        var result = _calculator.Calculate(space, start, end, createdAt);

        Assert.Equal(100m, result.BaseAmount);
        Assert.Equal(125m, result.FinalAmount);
        Assert.Contains(result.Adjustments, x => x.Rule == "PeakHour");
    }

    [Fact]
    public void Calculate_ShouldApplyWeekendSurcharge_WhenReservationIsOnWeekend()
    {
        var space = CreateSpace(100m);

        var createdAt = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
        var start = new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 6, 6, 13, 0, 0, TimeSpan.Zero);

        var result = _calculator.Calculate(space, start, end, createdAt);

        Assert.Equal(100m, result.BaseAmount);
        Assert.Equal(115m, result.FinalAmount);
        Assert.Contains(result.Adjustments, x => x.Rule == "Weekend");
    }

    [Fact]
    public void Calculate_ShouldApplyLongReservationDiscount_WhenDurationIsAtLeastFourHours()
    {
        var space = CreateSpace(100m);

        var createdAt = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
        var start = new DateTimeOffset(2026, 6, 2, 12, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 6, 2, 16, 0, 0, TimeSpan.Zero);

        var result = _calculator.Calculate(space, start, end, createdAt);

        Assert.Equal(400m, result.BaseAmount);
        Assert.Equal(360m, result.FinalAmount);
        Assert.Contains(result.Adjustments, x => x.Rule == "LongReservation");
    }

    [Fact]
    public void Calculate_ShouldApplyAdvanceBookingDiscount_WhenCreatedAtLeastSevenDaysBefore()
    {
        var space = CreateSpace(100m);

        var createdAt = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
        var start = new DateTimeOffset(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 6, 9, 13, 0, 0, TimeSpan.Zero);

        var result = _calculator.Calculate(space, start, end, createdAt);

        Assert.Equal(100m, result.BaseAmount);
        Assert.Equal(95m, result.FinalAmount);
        Assert.Contains(result.Adjustments, x => x.Rule == "AdvanceBooking");
    }

    [Fact]
    public void Calculate_ShouldApplyRulesInDocumentedOrder_WhenAllRulesApply()
    {
        var space = CreateSpace(100m);

        var createdAt = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
        var start = new DateTimeOffset(2026, 6, 13, 9, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 6, 13, 13, 0, 0, TimeSpan.Zero);

        var result = _calculator.Calculate(space, start, end, createdAt);

        Assert.Equal(400m, result.BaseAmount);
        Assert.Equal(491.63m, result.FinalAmount);

        Assert.Collection(
            result.Adjustments,
            first => Assert.Equal("PeakHour", first.Rule),
            second => Assert.Equal("Weekend", second.Rule),
            third => Assert.Equal("LongReservation", third.Rule),
            fourth => Assert.Equal("AdvanceBooking", fourth.Rule));
    }

    private static Space CreateSpace(decimal baseHourlyRate)
    {
        return new Space(
            id: Guid.NewGuid(),
            name: "Meeting Room A",
            capacity: 8,
            baseHourlyRate: baseHourlyRate,
            openingTime: new TimeOnly(8, 0),
            closingTime: new TimeOnly(20, 0),
            status: SpaceStatus.Active);
    }
}