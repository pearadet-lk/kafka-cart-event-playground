using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.EventBus;

public abstract class KafkaConsumerHostedService<T> : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly string _topic;

    protected KafkaConsumerHostedService(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        string topic)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _topic = topic;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = $"{_topic}-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = consumer.Consume(stoppingToken);
            using var scope = _scopeFactory.CreateScope();
            await ProcessMessageAsync(result.Message.Value, scope.ServiceProvider);
        }
    }

    protected abstract Task ProcessMessageAsync(string message, IServiceProvider serviceProvider);
}
