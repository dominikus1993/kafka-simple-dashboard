using System;

namespace KafkaSimpleDashboard.Server.Infrastructure.Config
{
    public class KafkaConsumerConfig
    {
        public string ClientId => $"{GroupId}{Guid.NewGuid()}";
        public string KafkaBrokers { get; set; }
        public string GroupId { get; set; }
    }
}