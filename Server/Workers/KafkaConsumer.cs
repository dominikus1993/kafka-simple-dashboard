using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaSimpleDashboard.Server.Infrastructure.Config;
using KafkaSimpleDashboard.Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaSimpleDashboard.Server.Workers
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly IList<KafkaSubscription> _kafkaSubscriptions;
        private readonly ILogger<KafkaConsumer> _logger;
        private readonly Channel<ConsumedKafkaMessage> _channel;
        private readonly IConsumer<Ignore, string> _consumer;

        public KafkaConsumer(ConsumerConfig consumerConfig, IOptions<KafkaSubscriptionConfig> config,
            ILogger<KafkaConsumer> logger, Channel<ConsumedKafkaMessage> channel)
        {
            _kafkaSubscriptions = config.Value.Subscriptions;
            _logger = logger;
            _channel = channel;
            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Error: {Reason}", e.Reason))
                .SetStatisticsHandler((_, json) => _logger.LogTrace("Statistics: {Stats}", json))
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    _logger.LogWarning("ConsumerId: {ConsumerId}, Assigned partitions: {Partitions}]", c.MemberId,
                        partitions);
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    _logger.LogWarning("ConsumerId: {ConsumerId}, Revoking assignment: {Partitions}]", c.MemberId,
                        partitions);
                }).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_kafkaSubscriptions.Select(x => x.Topic).ToArray());
            _logger.LogInformation("KafkaConsumer Started");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Yield();
                try
                {
                    var cr = _consumer.Consume(stoppingToken);
                    await _channel.Writer.WriteAsync(new ConsumedKafkaMessage
                    {
                        Topic = cr.Topic,
                        Body = cr.Message.Value,
                        Id = Guid.NewGuid(),
                        ConsumedAt = DateTime.UtcNow
                    }, stoppingToken);
                }
                catch (OperationCanceledException e)
                {
                    _logger.LogError(e, "OperationCanceled error");
                    throw;
                }
                catch (ConsumeException e)
                {
                    if (e.Error.IsFatal)
                    {
                        _logger.LogCritical(e, "kafka consumer fatal exception");
                        throw;
                    }

                    _logger.LogError(e, "Consume error");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error");
                    break;
                }
            }
            _logger.LogInformation("KafkaConsumer Stopped");
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _consumer.Close(); // Commit offsets and leave the group cleanly.
                _consumer.Dispose();
                _channel.Writer.Complete();
            }
        }

        public sealed override void Dispose()
        {
            Dispose(true);
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}