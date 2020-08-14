using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using KafkaSimpleDashboard.Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaSimpleDashboard.Server.Workers
{
    public class SigalRPublisher: BackgroundService
    {
        private Channel<KafkaMessage> _channel;
        private ILogger<SigalRPublisher> _logger;

        public SigalRPublisher(Channel<KafkaMessage> channel, ILogger<SigalRPublisher> logger)
        {
            _channel = channel;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var msg in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                _logger.LogInformation("Message received, {Msg}", msg);
            }
        }
    }
}