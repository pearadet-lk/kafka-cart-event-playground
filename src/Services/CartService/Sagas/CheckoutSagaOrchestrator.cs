using BuildingBlocks.EventBus;
using CartService.Models;
using System.Net.Http.Json;

namespace CartService.Sagas;

public sealed class CheckoutSagaOrchestrator(
    IHttpClientFactory httpClientFactory,
    IKafkaProducer kafkaProducer,
    ILogger<CheckoutSagaOrchestrator> logger) : ICheckoutSagaOrchestrator
{
    public async Task<CheckoutResult> ExecuteAsync(CheckoutRequest request, CancellationToken cancellationToken)
    {
        var sagaId = Guid.NewGuid();
        var inventoryReserved = false;
        var paymentProcessed = false;
        logger.LogInformation(
            "Starting checkout saga {SagaId} for cart {CartId} and user {UserId}.",
            sagaId,
            request.CartId,
            request.UserId);

        try
        {
            await ReserveInventoryAsync(request, sagaId, cancellationToken);
            inventoryReserved = true;

            await ProcessPaymentAsync(request, sagaId, cancellationToken);
            paymentProcessed = true;

            await CreateOrderAsync(request, sagaId, cancellationToken);

            await kafkaProducer.ProduceAsync("checkout.completed", new
            {
                SagaId = sagaId,
                request.CartId,
                request.UserId,
                request.TotalAmount,
                request.Currency
            });
            logger.LogInformation("Checkout saga {SagaId} completed successfully.", sagaId);

            return new CheckoutResult(true, "Checkout completed successfully.", sagaId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Checkout saga {SagaId} failed. Starting compensations.", sagaId);

            if (paymentProcessed)
            {
                await kafkaProducer.ProduceAsync("payment.refund", new
                {
                    SagaId = sagaId,
                    request.CartId,
                    request.UserId,
                    request.TotalAmount
                });
            }

            if (inventoryReserved)
            {
                await kafkaProducer.ProduceAsync("inventory.release", new
                {
                    SagaId = sagaId,
                    request.CartId,
                    request.UserId
                });
            }

            await kafkaProducer.ProduceAsync("checkout.failed", new
            {
                SagaId = sagaId,
                request.CartId,
                request.UserId,
                Reason = ex.Message
            });
            logger.LogInformation("Checkout saga {SagaId} marked as failed.", sagaId);

            return new CheckoutResult(false, $"Checkout failed: {ex.Message}", sagaId);
        }
    }

    private async Task ReserveInventoryAsync(CheckoutRequest request, Guid sagaId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Saga {SagaId}: reserving inventory.", sagaId);
        var client = httpClientFactory.CreateClient("inventory");
        var response = await client.PostAsJsonAsync(
            "/api/inventory/reserve",
            new { request.CartId, request.UserId, SagaId = sagaId },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task ProcessPaymentAsync(CheckoutRequest request, Guid sagaId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Saga {SagaId}: processing payment.", sagaId);
        var client = httpClientFactory.CreateClient("payment");
        var response = await client.PostAsJsonAsync(
            "/api/payment/process",
            new { request.CartId, request.UserId, request.TotalAmount, request.Currency, SagaId = sagaId },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task CreateOrderAsync(CheckoutRequest request, Guid sagaId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Saga {SagaId}: creating order.", sagaId);
        var client = httpClientFactory.CreateClient("order");
        var response = await client.PostAsJsonAsync(
            "/api/orders/create",
            new { request.CartId, request.UserId, request.TotalAmount, request.Currency, SagaId = sagaId },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
