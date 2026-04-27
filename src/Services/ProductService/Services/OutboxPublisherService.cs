using BuildingBlocks.EventBus;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;

namespace ProductService.Services;

public class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    IKafkaProducer producer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

            var pending = await db.OutboxMessages
                .Where(x => x.Status == "Pending")
                .ToListAsync(stoppingToken);

            foreach (var msg in pending)
            {
                await producer.ProduceAsync(msg.EventType, msg.Payload);
                msg.Status = "Sent";
            }

            await db.SaveChangesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
