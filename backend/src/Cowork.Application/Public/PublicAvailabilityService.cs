using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;

namespace Cowork.Application.Public;

public sealed class PublicAvailabilityService
{
    private static readonly TimeSpan SlotSize = TimeSpan.FromMinutes(30);

    private readonly ISpaceRepository _spaceRepository;
    private readonly IReservationRepository _reservationRepository;

    public PublicAvailabilityService(
        ISpaceRepository spaceRepository,
        IReservationRepository reservationRepository)
    {
        _spaceRepository = spaceRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<PublicAvailabilityResponse> GetAvailabilityAsync(
        Guid spaceId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        if (spaceId == Guid.Empty)
            throw new BusinessRuleException("Space id is required.");

        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        var timeZone = ResolveTimeZone(space.TimeZoneId);

        var dayStartUtc = ConvertLocalDateTimeToUtc(
            date,
            space.OpeningTime,
            timeZone);

        var dayEndUtc = ConvertLocalDateTimeToUtc(
            date,
            space.ClosingTime,
            timeZone);

        var reservations = await _reservationRepository.ListBySpaceAndRangeAsync(
            space.Id,
            dayStartUtc,
            dayEndUtc,
            cancellationToken);

        var reservedSlots = reservations
            .Select(reservation =>
            {
                var localStart = TimeZoneInfo.ConvertTime(reservation.StartTime, timeZone);
                var localEnd = TimeZoneInfo.ConvertTime(reservation.EndTime, timeZone);

                return new PublicReservedSlotDto(
                    TimeOnly.FromDateTime(localStart.DateTime),
                    TimeOnly.FromDateTime(localEnd.DateTime));
            })
            .OrderBy(x => x.StartTime)
            .ToList();

        var slots = BuildSlots(
            space,
            reservedSlots);

        return new PublicAvailabilityResponse(
            space.Id,
            space.Name,
            date,
            space.OpeningTime,
            space.ClosingTime,
            space.TimeZoneId,
            slots,
            reservedSlots);
    }

    private static IReadOnlyList<PublicAvailabilitySlotDto> BuildSlots(
        Space space,
        IReadOnlyList<PublicReservedSlotDto> reservedSlots)
    {
        var slots = new List<PublicAvailabilitySlotDto>();

        var current = space.OpeningTime;

        while (current < space.ClosingTime)
        {
            var next = current.Add(SlotSize);

            if (next > space.ClosingTime)
                break;

            var isAvailable = !reservedSlots.Any(reserved =>
                current < reserved.EndTime &&
                next > reserved.StartTime);

            slots.Add(new PublicAvailabilitySlotDto(
                current,
                next,
                isAvailable));

            current = next;
        }

        return slots;
    }

    private static DateTimeOffset ConvertLocalDateTimeToUtc(
        DateOnly date,
        TimeOnly time,
        TimeZoneInfo timeZone)
    {
        var localDateTime = date.ToDateTime(time, DateTimeKind.Unspecified);
        var offset = timeZone.GetUtcOffset(localDateTime);

        return new DateTimeOffset(localDateTime, offset).ToUniversalTime();
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException) when (timeZoneId == "America/Lima")
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        }
        catch (InvalidTimeZoneException) when (timeZoneId == "America/Lima")
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        }
    }
}