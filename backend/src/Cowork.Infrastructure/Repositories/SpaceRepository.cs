using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;
using Cowork.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cowork.Infrastructure.Repositories;

public sealed class SpaceRepository : ISpaceRepository
{
    private readonly CoworkDbContext _dbContext;

    public SpaceRepository(CoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Space>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Spaces
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Space?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Spaces
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Add(Space space)
    {
        _dbContext.Spaces.Add(space);
    }
}