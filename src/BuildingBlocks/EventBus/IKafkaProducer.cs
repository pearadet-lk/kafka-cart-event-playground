namespace BuildingBlocks.EventBus;

public interface IKafkaProducer
{
    Task ProduceAsync<T>(string topic, T message);
}
