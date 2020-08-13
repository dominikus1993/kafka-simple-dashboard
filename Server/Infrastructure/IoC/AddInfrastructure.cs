using System;
using Confluent.Kafka;
using KafkaSimpleDashboard.Server.Infrastructure.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaSimpleDashboard.Server.Infrastructure.IoC
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var cfg = configuration.GetValue<KafkaConsumerConfig>("Kafka");
            services.AddSingleton<ConsumerConfig>(x => new ConsumerConfig
            {
                SecurityProtocol = SecurityProtocol.Plaintext,
                ClientId = cfg.ClientId,
                BootstrapServers = cfg.KafkaBrokers,
                GroupId = cfg.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            });
            return services;
        }
    }
}