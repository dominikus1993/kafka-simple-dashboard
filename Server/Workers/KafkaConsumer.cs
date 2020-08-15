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
        private readonly ConsumerConfig _consumerConfig;
        private readonly IList<KafkaSubscription> _kafkaSubscriptions;
        private readonly ILogger<KafkaConsumer> _logger;
        private readonly Channel<KafkaMessage> _channel;

        public KafkaConsumer(ConsumerConfig consumerConfig, IOptions<KafkaSubscriptionConfig> config,
            ILogger<KafkaConsumer> logger, Channel<KafkaMessage> channel)
        {
            _consumerConfig = consumerConfig;
            _kafkaSubscriptions = config.Value.Subscriptions;
            _logger = logger;
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig)
                // Note: All handlers are called on the main .Consume thread.
                .SetErrorHandler((_, e) => _logger.LogError($"Error: {e.Reason}"))
                .SetStatisticsHandler((_, json) => _logger.LogTrace($"Statistics: {json}"))
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    _logger.LogWarning($"Assigned partitions: [{string.Join(", ", partitions)}]");
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    _logger.LogWarning($"Revoking assignment: [{string.Join(", ", partitions)}]");
                })
                .Build();

            consumer.Subscribe(_kafkaSubscriptions.Select(x => x.Topic).ToArray());
            try
            {
                await Task.Yield();
                while (true)
                {
                    try
                    {
                        var cr = consumer.Consume(stoppingToken);
                        await _channel.Writer.WriteAsync(new KafkaMessage
                        {
                            Topic = cr.Topic,
                            Body = cr.Message.Value,
                            Id = Guid.NewGuid(),
                            ConsumedAt = DateTime.UtcNow
                        }, stoppingToken);
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError(e, $"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
                _channel.Writer.Complete();
                throw;
            }
        }
    }
}