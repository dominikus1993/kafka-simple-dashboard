using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace KafkaSimpleDashboard.Server.Workers
{
    public class KafkaConsumer : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}