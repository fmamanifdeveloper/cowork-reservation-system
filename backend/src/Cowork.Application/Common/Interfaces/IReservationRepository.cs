using Cowork.Domain.Entities;

namespace Cowork.Application.Common.Interfaces;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> ListAsync(CancellationToken cancellationToken);
    void Add(Reservation reservation);
}