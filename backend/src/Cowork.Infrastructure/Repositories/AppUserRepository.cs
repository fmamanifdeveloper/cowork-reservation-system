using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;
using Cowork.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cowork.Infrastructure.Repositories;

public sealed class AppUserRepository : IAppUserRepository
{
    private readonly CoworkDbContext _dbContext;

    public AppUserRepository(CoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.AppUsers
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await _dbContext.AppUsers
            .FirstOrDefaultAsync(
                x => x.Username == username && !x.IsDeleted,
                cancellationToken);
    }
}