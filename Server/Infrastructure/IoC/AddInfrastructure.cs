using System;
using System.Threading.Channels;
using Confluent.Kafka;
using KafkaSimpleDashboard.Server.Infrastructure.Config;
using KafkaSimpleDashboard.Server.Workers;
using KafkaSimpleDashboard.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaSimpleDashboard.Server.Infrastructure.IoC
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var cfg = configuration.GetSection("Kafka").Get<KafkaConsumerConfig>();
            services.AddSingleton<ConsumerConfig>(x => new ConsumerConfig
            {
                SecurityProtocol = SecurityProtocol.Plaintext,
                ClientId = cfg.ClientId,
                BootstrapServers = cfg.KafkaBrokers,
                GroupId = cfg.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            });
            services.Configure<KafkaSubscriptionConfig>(configuration.GetSection("Kafka"));
            services.AddSingleton<Channel<KafkaMessage>>(_ => Channel.CreateUnbounded<KafkaMessage>());
            services.AddHostedService<KafkaConsumer>();
            services.AddHostedService<SigalRPublisher>();
            return services;
        }
    }
}