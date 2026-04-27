using BuildingBlocks.EventBus;
using BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args)
    .AddObservability("ProductService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<OutboxPublisherService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.EnsureCreated();
}

app.UseDefaultExceptionHandling();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("ProductService is healthy"));

app.Run();
