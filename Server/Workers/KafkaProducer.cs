using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaSimpleDashboard.Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaSimpleDashboard.Server.Workers
{
    public class KafkaProducer : BackgroundService
    {
        private readonly Channel<KafkaMessage> _channel;
        private readonly ProducerConfig _config;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(Channel<KafkaMessage> channel, ProducerConfig config, ILogger<KafkaProducer> logger)
        {
            _channel = channel;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("KafkaProducer Started");
            using var producer = new ProducerBuilder<Null, string>(_config).Build();
            await foreach (var msg in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                await producer.ProduceAsync(msg.Topic, new Message<Null, string> {Value = msg.Body}, stoppingToken);
                _logger.LogDebug("Message Published {Msg}", msg);
            }
            _logger.LogInformation("KafkaProducer Stopped");
        }
    }
}