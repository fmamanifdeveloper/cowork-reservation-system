using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;
using Cowork.Infrastructure.Persistence;

namespace Cowork.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly CoworkDbContext _dbContext;

    public AuditLogRepository(CoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(AuditLog auditLog)
    {
        _dbContext.AuditLogs.Add(auditLog);
    }
}