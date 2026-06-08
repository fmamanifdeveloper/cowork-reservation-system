using Cowork.Application.Cancellations;
using FluentAssertions;

namespace Cowork.UnitTests.Cancellations;

public sealed class CancellationPolicyServiceTests
{
    private readonly CancellationPolicyService _service = new();

    [Fact]
    public void CalculateRefund_ShouldReturnFullRefund_WhenCancelledMoreThanFortyEightHoursBeforeStart()
    {
        var cancellationRequestedAt = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);
        var reservationStart = cancellationRequestedAt.AddHours(49);
        var finalAmount = 200m;

        var result = _service.CalculateRefund(
            reservationStart,
            cancellationRequestedAt,
            finalAmount);

        result.RefundAmount.Should().Be(200m);
    }

    [Fact]
    public void CalculateRefund_ShouldReturnHalfRefund_WhenCancelledBetweenTwentyFourAndFortyEightHoursBeforeStart()
    {
        var cancellationRequestedAt = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);
        var reservationStart = cancellationRequestedAt.AddHours(36);
        var finalAmount = 200m;

        var result = _service.CalculateRefund(
            reservationStart,
            cancellationRequestedAt,
            finalAmount);

        result.RefundAmount.Should().Be(100m);
    }

    [Fact]
    public void CalculateRefund_ShouldReturnNoRefund_WhenCancelledLessThanTwentyFourHoursBeforeStart()
    {
        var cancellationRequestedAt = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);
        var reservationStart = cancellationRequestedAt.AddHours(12);
        var finalAmount = 200m;

        var result = _service.CalculateRefund(
            reservationStart,
            cancellationRequestedAt,
            finalAmount);

        result.RefundAmount.Should().Be(0m);
    }

    [Fact]
    public void CalculateRefund_ShouldReturnNoRefund_WhenCancellationIsRequestedAfterReservationStart()
    {
        var reservationStart = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);
        var cancellationRequestedAt = reservationStart.AddMinutes(30);
        var finalAmount = 200m;

        var result = _service.CalculateRefund(
            reservationStart,
            cancellationRequestedAt,
            finalAmount);

        result.RefundAmount.Should().Be(0m);
    }

    [Fact]
    public void CalculateRefund_ShouldRoundRefundAmountToTwoDecimals()
    {
        var cancellationRequestedAt = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);
        var reservationStart = cancellationRequestedAt.AddHours(36);
        var finalAmount = 99.99m;

        var result = _service.CalculateRefund(
            reservationStart,
            cancellationRequestedAt,
            finalAmount);

        result.RefundAmount.Should().Be(50.00m);
    }
}