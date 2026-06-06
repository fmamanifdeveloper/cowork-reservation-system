using Cowork.Application.Pricing;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[ApiController]
[Route("api/pricing")]
public sealed class PricingController : ControllerBase
{
    private readonly PricingPreviewService _pricingPreviewService;

    public PricingController(PricingPreviewService pricingPreviewService)
    {
        _pricingPreviewService = pricingPreviewService;
    }

    [HttpPost("preview")]
    public async Task<ActionResult<PricingPreviewResponse>> Preview(
        PricingPreviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _pricingPreviewService.PreviewAsync(request, cancellationToken);
        return Ok(result);
    }
}