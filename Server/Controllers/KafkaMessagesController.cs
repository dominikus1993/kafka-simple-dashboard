using System.Threading.Tasks;
using KafkaSimpleDashboard.Server.Services.Abstractions;
using KafkaSimpleDashboard.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KafkaSimpleDashboard.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KafkaMessagesController: ControllerBase
    {
        private IMessagePublisher _messagePublisher;
        private ILogger<KafkaMessagesController> _logger;
        public KafkaMessagesController(IMessagePublisher messagePublisher, ILogger<KafkaMessagesController> logger)
        {
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        [HttpPut]
        public async Task<IActionResult> Publish([FromBody] KafkaMessage msg)
        {
            await _messagePublisher.Publish(msg);
            return Ok();
        }
    }
}