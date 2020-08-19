using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaSimpleDashboard.Server.Services.Abstractions;
using KafkaSimpleDashboard.Shared;
using LanguageExt;

namespace KafkaSimpleDashboard.Server.Services.Implementations
{
    public class ConfluentKafkaTopicsAdminService : IKafkaTopicsAdminService
    {
        public async Task<Option<IList<KafkaTopic>>> GetAll()
        {
            throw new System.NotImplementedException();
        }
    }
}