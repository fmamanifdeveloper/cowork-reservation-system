using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;

namespace Cowork.Application.Spaces;

public sealed class SpaceService
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SpaceService(ISpaceRepository spaceRepository, IUnitOfWork unitOfWork)
    {
        _spaceRepository = spaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SpaceDto>> ListAsync(CancellationToken cancellationToken)
    {
        var spaces = await _spaceRepository.ListAsync(cancellationToken);
        return spaces.Select(ToDto).ToList();
    }

    public async Task<SpaceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var space = await _spaceRepository.GetByIdAsync(id, cancellationToken);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        return ToDto(space);
    }

    public async Task<SpaceDto> CreateAsync(CreateSpaceRequest request, CancellationToken cancellationToken)
    {
        var space = new Space(
            Guid.NewGuid(),
            request.Name,
            request.Capacity,
            request.BaseHourlyRate,
            request.OpeningTime,
            request.ClosingTime,
            request.Status);

        _spaceRepository.Add(space);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(space);
    }

    public async Task<SpaceDto> UpdateAsync(Guid id, UpdateSpaceRequest request, CancellationToken cancellationToken)
    {
        var space = await _spaceRepository.GetByIdAsync(id, cancellationToken);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        space.Update(
            request.Name,
            request.Capacity,
            request.BaseHourlyRate,
            request.OpeningTime,
            request.ClosingTime,
            request.Status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(space);
    }

    private static SpaceDto ToDto(Space space)
    {
        return new SpaceDto(
            space.Id,
            space.Name,
            space.Capacity,
            space.BaseHourlyRate,
            space.OpeningTime,
            space.ClosingTime,
            space.Status);
    }
}