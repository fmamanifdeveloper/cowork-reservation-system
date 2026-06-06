using Cowork.Domain.Entities;

namespace Cowork.Application.Common.Interfaces;

public interface ISpaceRepository
{
    Task<IReadOnlyList<Space>> ListAsync(CancellationToken cancellationToken);
    Task<Space?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(Space space);
}