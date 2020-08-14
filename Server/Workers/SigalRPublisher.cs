using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using KafkaSimpleDashboard.Server.Infrastructure.SignalR;
using KafkaSimpleDashboard.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaSimpleDashboard.Server.Workers
{
    public class SigalRPublisher: BackgroundService
    {
        private Channel<KafkaMessage> _channel;
        private ILogger<SigalRPublisher> _logger;
        private IHubContext<KafkaMessagesHub> _hubContext;

        public SigalRPublisher(Channel<KafkaMessage> channel, IHubContext<KafkaMessagesHub> hubContext, ILogger<SigalRPublisher> logger)
        {
            _channel = channel;
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var msg in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                _logger.LogInformation("Message received, {Msg}", msg);
                await _hubContext.Clients.All.SendAsync("ReceivedKafkaMessage", msg, cancellationToken: stoppingToken);
            }
        }
    }
}