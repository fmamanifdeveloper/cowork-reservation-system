namespace Cowork.Application.Cancellations;

public sealed record CancellationPolicyResult(
    decimal RefundPercentage,
    decimal RefundAmount,
    string Reason);