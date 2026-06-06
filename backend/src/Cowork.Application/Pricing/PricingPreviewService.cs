using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;

namespace Cowork.Application.Pricing;

public sealed class PricingPreviewService
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly DynamicPricingCalculator _pricingCalculator;

    public PricingPreviewService(
        ISpaceRepository spaceRepository,
        DynamicPricingCalculator pricingCalculator)
    {
        _spaceRepository = spaceRepository;
        _pricingCalculator = pricingCalculator;
    }

    public async Task<PricingPreviewResponse> PreviewAsync(
        PricingPreviewRequest request,
        CancellationToken cancellationToken)
    {
        var space = await _spaceRepository.GetByIdAsync(request.SpaceId, cancellationToken);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        if (!space.IsAvailableForReservation())
            throw new BusinessRuleException("The selected space is under maintenance and cannot receive reservations.");

        var result = _pricingCalculator.Calculate(
            space,
            request.StartTime,
            request.EndTime,
            DateTimeOffset.UtcNow);

        return new PricingPreviewResponse(
            result.BaseAmount,
            result.FinalAmount,
            result.Adjustments);
    }
}