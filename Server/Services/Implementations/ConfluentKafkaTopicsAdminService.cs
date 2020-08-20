using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaSimpleDashboard.Server.Services.Abstractions;
using KafkaSimpleDashboard.Shared;
using LanguageExt;
using static LanguageExt.Prelude;

namespace KafkaSimpleDashboard.Server.Services.Implementations
{
    public class ConfluentKafkaTopicsAdminService : IKafkaTopicsAdminService
    {
        private AdminClientConfig _adminClientConfig;

        public ConfluentKafkaTopicsAdminService(AdminClientConfig adminClientConfig)
        {
            _adminClientConfig = adminClientConfig;
        }

        public ValueTask<Option<List<KafkaTopic>>> GetAll()
        {
            using var admin = new AdminClientBuilder(_adminClientConfig).Build();
            var meta = admin.GetMetadata(TimeSpan.FromSeconds(10));
            var result = Optional(meta)
                .Map(m => m.Topics.Select(x => new KafkaTopic() {Name = x.Topic,}).ToList());
            return new ValueTask<Option<List<KafkaTopic>>>(result);
        }
    }
}