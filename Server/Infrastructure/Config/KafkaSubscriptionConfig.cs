using System.Collections.Generic;

namespace KafkaSimpleDashboard.Server.Infrastructure.Config
{
    public class KafkaSubscription
    {
        public string Topic { get; set; }
    }
    
    public class KafkaSubscriptionConfig
    {
        public IList<KafkaSubscription> Subscriptions { get; set; }
    }
}