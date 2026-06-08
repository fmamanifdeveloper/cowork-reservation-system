using Cowork.Domain.Entities;

namespace Cowork.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    void Add(AuditLog auditLog);
}