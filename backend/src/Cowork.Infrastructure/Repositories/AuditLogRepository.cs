using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;
using Cowork.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cowork.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly CoworkDbContext _dbContext;

    public AuditLogRepository(CoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuditLog>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public void Add(AuditLog auditLog)
    {
        _dbContext.AuditLogs.Add(auditLog);
    }
}