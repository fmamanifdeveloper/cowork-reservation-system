using Cowork.Domain.Entities;

namespace Cowork.Application.Common.Interfaces;

public interface IAppUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}