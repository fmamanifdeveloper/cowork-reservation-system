using Cowork.Application.Pricing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/public/pricing")]
public sealed class PublicPricingController : ControllerBase
{
    private readonly PricingPreviewService _pricingPreviewService;

    public PublicPricingController(PricingPreviewService pricingPreviewService)
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