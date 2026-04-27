using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BuildingBlocks.EventBus;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092"
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task ProduceAsync<T>(string topic, T message)
    {
        var payload = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = payload
            });
    }
}
