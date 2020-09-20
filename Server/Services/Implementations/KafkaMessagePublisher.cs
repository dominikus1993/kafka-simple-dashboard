using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaSimpleDashboard.Server.Services.Abstractions;
using KafkaSimpleDashboard.Shared;

namespace KafkaSimpleDashboard.Server.Services.Implementations
{
    public class KafkaMessagePublisher : IMessagePublisher
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaMessagePublisher(IProducer<Null, string> producer)
        {
            _producer = producer;
        }

        public async Task Publish(KafkaMessage message)
        {
            await _producer.ProduceAsync(message.Topic, new Message<Null, string>() {Value = message.Body});
        }
    }
}