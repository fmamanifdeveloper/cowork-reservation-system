using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;

namespace Cowork.Application.Spaces;

public sealed class SpaceService
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SpaceService(
        ISpaceRepository spaceRepository,
        IAuditLogger auditLogger,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _spaceRepository = spaceRepository;
        _auditLogger = auditLogger;
        _currentUserService = currentUserService;
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

    public async Task<SpaceDto> CreateAsync(
        CreateSpaceRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var space = new Space(
            Guid.NewGuid(),
            request.Name,
            request.Capacity,
            request.BaseHourlyRate,
            request.OpeningTime,
            request.ClosingTime,
            request.Status,
            request.TimeZoneId ?? "America/Lima",
            currentUserId);

        _spaceRepository.Add(space);

        await _auditLogger.LogAsync(
            "SpaceCreated",
            "Space",
            space.Id,
            currentUserId,
            null,
            "Create",
            "Space was created.",
            null,
            new
            {
                space.Id,
                space.Name,
                space.Capacity,
                space.BaseHourlyRate,
                space.OpeningTime,
                space.ClosingTime,
                space.TimeZoneId,
                space.Status,
                space.CreatedByUserId
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(space);
    }

    public async Task<SpaceDto> UpdateAsync(
        Guid id,
        UpdateSpaceRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var space = await _spaceRepository.GetByIdAsync(id, cancellationToken);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        var oldValues = new
        {
            space.Name,
            space.Capacity,
            space.BaseHourlyRate,
            space.OpeningTime,
            space.ClosingTime,
            space.TimeZoneId,
            space.Status,
            space.UpdatedByUserId
        };

        space.Update(
            request.Name,
            request.Capacity,
            request.BaseHourlyRate,
            request.OpeningTime,
            request.ClosingTime,
            request.Status,
            request.TimeZoneId ?? "America/Lima",
            currentUserId);

        await _auditLogger.LogAsync(
            "SpaceUpdated",
            "Space",
            space.Id,
            currentUserId,
            null,
            "Update",
            "Space was updated.",
            oldValues,
            new
            {
                space.Name,
                space.Capacity,
                space.BaseHourlyRate,
                space.OpeningTime,
                space.ClosingTime,
                space.TimeZoneId,
                space.Status,
                space.UpdatedByUserId
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(space);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var space = await _spaceRepository.GetByIdAsync(id, cancellationToken);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        var oldValues = new
        {
            space.Name,
            space.Status,
            space.IsDeleted,
            space.DeletedAt,
            space.DeletedByUserId
        };

        space.Delete(currentUserId);

        await _auditLogger.LogAsync(
            "SpaceDeleted",
            "Space",
            space.Id,
            currentUserId,
            null,
            "Delete",
            "Space was logically deleted.",
            oldValues,
            new
            {
                space.Name,
                space.Status,
                space.IsDeleted,
                space.DeletedAt,
                space.DeletedByUserId
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
            space.TimeZoneId,
            space.Status);
    }
}