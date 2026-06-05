namespace Cowork.Application.Cancellations;

public sealed class CancellationPolicyService
{
    public CancellationPolicyResult CalculateRefund(
        DateTimeOffset reservationStartTime,
        DateTimeOffset cancellationRequestedAt,
        decimal reservationFinalAmount)
    {
        if (reservationFinalAmount < 0)
            throw new ArgumentException("Reservation final amount cannot be negative.", nameof(reservationFinalAmount));

        var hoursBeforeStart = (reservationStartTime - cancellationRequestedAt).TotalHours;

        if (hoursBeforeStart > 48)
        {
            return BuildResult(
                reservationFinalAmount,
                1.00m,
                "Cancellation requested more than 48 hours before start time.");
        }

        if (hoursBeforeStart >= 24)
        {
            return BuildResult(
                reservationFinalAmount,
                0.50m,
                "Cancellation requested between 24 and 48 hours before start time.");
        }

        return BuildResult(
            reservationFinalAmount,
            0.00m,
            "Cancellation requested less than 24 hours before start time.");
    }

    private static CancellationPolicyResult BuildResult(
        decimal reservationFinalAmount,
        decimal refundPercentage,
        string reason)
    {
        var refundAmount = Math.Round(
            reservationFinalAmount * refundPercentage,
            2,
            MidpointRounding.AwayFromZero);

        return new CancellationPolicyResult(
            RefundPercentage: refundPercentage,
            RefundAmount: refundAmount,
            Reason: reason);
    }
}