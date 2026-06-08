using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;
using Cowork.Domain.Enums;
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

    public async Task<IReadOnlyList<Reservation>> ListByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
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
        var fromUtc = from.ToUniversalTime();
        var toUtc = to.ToUniversalTime();

        return await _dbContext.Reservations
            .AsNoTracking()
            .Where(x => x.StartTime < toUtc && x.EndTime > fromUtc)
            .OrderBy(x => x.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsOverlappingAsync(
        Guid spaceId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken)
    {
        var startTimeUtc = startTime.ToUniversalTime();
        var endTimeUtc = endTime.ToUniversalTime();

        return await _dbContext.Reservations
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.SpaceId == spaceId &&
                    x.Status != ReservationStatus.Cancelled &&
                    x.StartTime < endTimeUtc &&
                    x.EndTime > startTimeUtc,
                cancellationToken);
    }

    public void Add(Reservation reservation)
    {
        _dbContext.Reservations.Add(reservation);
    }
}