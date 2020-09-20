using System.Threading.Channels;
using System.Threading.Tasks;
using KafkaSimpleDashboard.Server.Services.Abstractions;
using KafkaSimpleDashboard.Shared;

namespace KafkaSimpleDashboard.Server.Services.Implementations
{
    public class KafkaMessagePublisher : IMessagePublisher
    {
        private readonly Channel<KafkaMessage> _channel;

        public KafkaMessagePublisher(Channel<KafkaMessage> channel)
        {
            _channel = channel;
        }


        public async Task Publish(KafkaMessage message)
        {
            await _channel.Writer.WriteAsync(message);
        }
    }
}