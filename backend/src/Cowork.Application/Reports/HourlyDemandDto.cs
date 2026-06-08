namespace Cowork.Application.Reports;

public sealed record HourlyDemandDto(
    int Hour,
    int ReservationCount);