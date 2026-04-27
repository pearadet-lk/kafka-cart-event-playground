namespace CartService.Sagas;

public sealed record CheckoutResult(bool Success, string Message, Guid SagaId);
