using CartService.Models;
using CartService.Sagas;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Controllers;

[ApiController]
[Route("api/checkout")]
public sealed class CheckoutController(
    ICheckoutSagaOrchestrator sagaOrchestrator,
    ILogger<CheckoutController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received checkout request for cart {CartId}.", request.CartId);
        var result = await sagaOrchestrator.ExecuteAsync(request, cancellationToken);
        if (!result.Success)
        {
            logger.LogWarning("Checkout failed for saga {SagaId}.", result.SagaId);
            return BadRequest(result);
        }

        logger.LogDebug("Checkout succeeded for saga {SagaId}.", result.SagaId);
        return Ok(result);
    }
}
