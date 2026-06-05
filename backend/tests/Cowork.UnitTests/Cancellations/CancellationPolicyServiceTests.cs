using Cowork.Application.Cancellations;

namespace Cowork.UnitTests.Cancellations;

public sealed class CancellationPolicyServiceTests
{
    private readonly CancellationPolicyService _service = new();

    [Fact]
    public void CalculateRefund_ShouldReturnFullRefund_WhenCancellationIsMoreThan48HoursBefore()
    {
        var reservationStart = new DateTimeOffset(2026, 6, 10, 10, 0, 0, TimeSpan.Zero);
        var cancellationDate = new DateTimeOffset(2026, 6, 8, 9, 59, 0, TimeSpan.Zero);

        var result = _service.CalculateRefund(reservationStart, cancellationDate, 200m);

        Assert.Equal(1.00m, result.RefundPercentage);
        Assert.Equal(200m, result.RefundAmount);
    }

    [Fact]
    public void CalculateRefund_ShouldReturnHalfRefund_WhenCancellationIsExactly48HoursBefore()
    {
        var reservationStart = new DateTimeOffset(2026, 6, 10, 10, 0, 0, TimeSpan.Zero);
        var cancellationDate = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);

        var result = _service.CalculateRefund(reservationStart, cancellationDate, 200m);

        Assert.Equal(0.50m, result.RefundPercentage);
        Assert.Equal(100m, result.RefundAmount);
    }

    [Fact]
    public void CalculateRefund_ShouldReturnHalfRefund_WhenCancellationIsBetween24And48HoursBefore()
    {
        var reservationStart = new DateTimeOffset(2026, 6, 10, 10, 0, 0, TimeSpan.Zero);
        var cancellationDate = new DateTimeOffset(2026, 6, 9, 0, 0, 0, TimeSpan.Zero);

        var result = _service.CalculateRefund(reservationStart, cancellationDate, 200m);

        Assert.Equal(0.50m, result.RefundPercentage);
        Assert.Equal(100m, result.RefundAmount);
    }

    [Fact]
    public void CalculateRefund_ShouldReturnNoRefund_WhenCancellationIsLessThan24HoursBefore()
    {
        var reservationStart = new DateTimeOffset(2026, 6, 10, 10, 0, 0, TimeSpan.Zero);
        var cancellationDate = new DateTimeOffset(2026, 6, 9, 11, 0, 0, TimeSpan.Zero);

        var result = _service.CalculateRefund(reservationStart, cancellationDate, 200m);

        Assert.Equal(0.00m, result.RefundPercentage);
        Assert.Equal(0m, result.RefundAmount);
    }
}