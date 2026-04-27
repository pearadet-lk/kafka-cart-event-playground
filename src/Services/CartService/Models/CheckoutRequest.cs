namespace CartService.Models;

public sealed class CheckoutRequest
{
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
}
