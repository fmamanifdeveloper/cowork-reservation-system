using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Cowork.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CoworkDbContext _dbContext;

    public UnitOfWork(CoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsExclusionViolation(exception))
        {
            throw new ReservationConflictException(
                "The selected time slot is already reserved for this space.",
                exception);
        }
    }

    private static bool IsExclusionViolation(Exception exception)
    {
        var current = exception;

        while (current is not null)
        {
            if (current is PostgresException postgresException &&
                postgresException.SqlState == PostgresErrorCodes.ExclusionViolation)
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }
}