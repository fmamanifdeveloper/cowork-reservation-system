using Cowork.Domain.Entities;

namespace Cowork.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLog>> ListAsync(CancellationToken cancellationToken);
    void Add(AuditLog auditLog);
}