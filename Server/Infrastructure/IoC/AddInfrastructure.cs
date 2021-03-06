﻿using System;
using System.Threading.Channels;
using Confluent.Kafka;
using KafkaSimpleDashboard.Server.Infrastructure.Config;
using KafkaSimpleDashboard.Server.Services.Abstractions;
using KafkaSimpleDashboard.Server.Services.Implementations;
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
            services.AddSingleton<AdminClientConfig>(x => new AdminClientConfig
            {
                SecurityProtocol = SecurityProtocol.Plaintext,
                ClientId = cfg.ClientId,
                BootstrapServers = cfg.KafkaBrokers,
            });

            services.AddSingleton<ProducerConfig>(sp => new ProducerConfig()
            {
                SecurityProtocol = SecurityProtocol.Plaintext,
                ClientId = cfg.ClientId,
                BootstrapServers = cfg.KafkaBrokers,
            });
            services.AddSingleton<ProducerBuilder<Null, string>>(sp => new ProducerBuilder<Null, string>(sp.GetService<ProducerConfig>()));
            services.AddScoped<IProducer<Null, string>>(sp =>
            {
                var builder = sp.GetService<ProducerBuilder<Null, string>>();
                return builder.Build();
            });
            services.Configure<KafkaSubscriptionConfig>(configuration.GetSection("Kafka"));
            services.AddSingleton<Channel<ConsumedKafkaMessage>>(_ => Channel.CreateUnbounded<ConsumedKafkaMessage>());
            services.AddSingleton<Channel<KafkaMessage>>(_ => Channel.CreateUnbounded<KafkaMessage>());
            services.AddHostedService<KafkaConsumer>();
            services.AddHostedService<KafkaProducer>();
            services.AddHostedService<SigalRPublisher>();
            services.AddScoped<IKafkaTopicsAdminService, ConfluentKafkaTopicsAdminService>();
            services.AddScoped<IMessagePublisher, KafkaMessagePublisher>();
            return services;
        }
    }
}