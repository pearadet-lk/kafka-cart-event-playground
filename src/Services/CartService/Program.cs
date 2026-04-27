using BuildingBlocks.EventBus;
using BuildingBlocks.Observability;
using CartService.Sagas;

var builder = WebApplication.CreateBuilder(args)
    .AddObservability("CartService");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();
builder.Services.AddScoped<ICheckoutSagaOrchestrator, CheckoutSagaOrchestrator>();
builder.Services.AddPollyRetryHttpClient(
    "inventory",
    builder.Configuration["Services:InventoryBaseUrl"] ?? "http://inventoryservice:8080",
    GetResilienceOptions("Inventory"));
builder.Services.AddPollyRetryHttpClient(
    "payment",
    builder.Configuration["Services:PaymentBaseUrl"] ?? "http://paymentservice:8080",
    GetResilienceOptions("Payment"));
builder.Services.AddPollyRetryHttpClient(
    "order",
    builder.Configuration["Services:OrderBaseUrl"] ?? "http://orderservice:8080",
    GetResilienceOptions("Order"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultExceptionHandling();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("CartService is healthy"));

app.Run();

ResiliencePolicyOptions GetResilienceOptions(string clientName)
{
    var options = new ResiliencePolicyOptions();
    builder.Configuration.GetSection("Resilience:Default").Bind(options);
    builder.Configuration.GetSection($"Resilience:{clientName}").Bind(options);
    return options;
}
