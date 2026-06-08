using Cowork.Application.Cancellations;

namespace Cowork.UnitTests.Cancellations;

public sealed class CancellationPolicyServiceTests
{
    private readonly CancellationPolicyService _service = new();

    [Fact]
    public void CalculateRefund_ShouldReturnFullRefund_WhenCancelledMoreThanFortyEightHoursBeforeStart()
    {
        var reservationStartTime = new DateTimeOffset(2026, 7, 20, 10, 0, 0, TimeSpan.Zero);
        var cancellationRequestedAt = reservationStartTime.AddHours(-49);
        var finalAmount = 1000m;

        var result = _service.CalculateRefund(
            reservationStartTime,
            cancellationRequestedAt,
            finalAmount);

        Assert.Equal(1000m, result.RefundAmount);
    }

    [Fact]
    public void CalculateRefund_ShouldReturnHalfRefund_WhenCancelledBetweenTwentyFourAndFortyEightHoursBeforeStart()
    {
        var reservationStartTime = new DateTimeOffset(2026, 7, 20, 10, 0, 0, TimeSpan.Zero);
        var cancellationRequestedAt = reservationStartTime.AddHours(-36);
        var finalAmount = 1000m;

        var result = _service.CalculateRefund(
            reservationStartTime,
            cancellationRequestedAt,
            finalAmount);

        Assert.Equal(500m, result.RefundAmount);
    }

    [Fact]
    public void CalculateRefund_ShouldReturnZeroRefund_WhenCancelledLessThanTwentyFourHoursBeforeStart()
    {
        var reservationStartTime = new DateTimeOffset(2026, 7, 20, 10, 0, 0, TimeSpan.Zero);
        var cancellationRequestedAt = reservationStartTime.AddHours(-12);
        var finalAmount = 1000m;

        var result = _service.CalculateRefund(
            reservationStartTime,
            cancellationRequestedAt,
            finalAmount);

        Assert.Equal(0m, result.RefundAmount);
    }

    [Fact]
    public void CalculateRefund_ShouldReturnZeroRefund_WhenCancellationIsAfterReservationStart()
    {
        var reservationStartTime = new DateTimeOffset(2026, 7, 20, 10, 0, 0, TimeSpan.Zero);
        var cancellationRequestedAt = reservationStartTime.AddHours(1);
        var finalAmount = 1000m;

        var result = _service.CalculateRefund(
            reservationStartTime,
            cancellationRequestedAt,
            finalAmount);

        Assert.Equal(0m, result.RefundAmount);
    }
}