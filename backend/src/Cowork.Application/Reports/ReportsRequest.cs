namespace Cowork.Application.Reports;

public sealed record ReportsRequest(
    DateTimeOffset From,
    DateTimeOffset To);