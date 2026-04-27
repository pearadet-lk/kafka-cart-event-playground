using CartService.Models;

namespace CartService.Sagas;

public interface ICheckoutSagaOrchestrator
{
    Task<CheckoutResult> ExecuteAsync(CheckoutRequest request, CancellationToken cancellationToken);
}
