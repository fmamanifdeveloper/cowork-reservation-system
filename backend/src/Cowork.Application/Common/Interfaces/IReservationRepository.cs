using Cowork.Domain.Entities;

namespace Cowork.Application.Common.Interfaces;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Reservation>> ListByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    Task<Reservation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Reservation>> ListByRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Reservation>> ListBySpaceAndRangeAsync(
        Guid spaceId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);

    Task<bool> ExistsOverlappingAsync(
        Guid spaceId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken);

    void Add(Reservation reservation);
}