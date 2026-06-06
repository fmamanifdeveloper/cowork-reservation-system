using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;
using Cowork.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cowork.Infrastructure.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly CoworkDbContext _dbContext;

    public ReservationRepository(CoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Reservation>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> ListByRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .Where(x => x.StartTime < to && x.EndTime > from)
            .OrderBy(x => x.StartTime)
            .ToListAsync(cancellationToken);
    }

    public void Add(Reservation reservation)
    {
        _dbContext.Reservations.Add(reservation);
    }
}