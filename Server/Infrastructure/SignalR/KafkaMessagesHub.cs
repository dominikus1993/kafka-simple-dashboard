using System.Threading.Tasks;
using KafkaSimpleDashboard.Shared;
using Microsoft.AspNetCore.SignalR;

namespace KafkaSimpleDashboard.Server.Infrastructure.SignalR
{
    public class KafkaMessagesHub : Hub
    {
        public async Task PublishKafkaMessage(KafkaMessage msg)
        {
            await Clients.All.SendAsync("ReceivedKafkaMessage", msg);
        } 
    }
}