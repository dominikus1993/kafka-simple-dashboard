using System.Threading.Tasks;
using KafkaSimpleDashboard.Shared;

namespace KafkaSimpleDashboard.Server.Services.Abstractions
{
    public interface IMessagePublisher
    {
        Task Publish(KafkaMessage message);
    }
}