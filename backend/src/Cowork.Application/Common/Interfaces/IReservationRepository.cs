using Cowork.Domain.Entities;

namespace Cowork.Application.Common.Interfaces;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Reservation>> ListByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Reservation>> ListByRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);

    void Add(Reservation reservation);
}