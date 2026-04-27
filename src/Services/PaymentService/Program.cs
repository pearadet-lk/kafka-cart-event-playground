using BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args)
    .AddObservability("PaymentService");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapGet("/health", () => Results.Ok("PaymentService is healthy"));

app.Run();
