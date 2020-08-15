using System;

namespace KafkaSimpleDashboard.Shared
{
    public class KafkaMessage
    {
        public string Body { get; set; }
        public string Topic { get; set; }
        public DateTime ConsumedAt { get; set; }
        public Guid Id { get; set; }

        public override string ToString()
        {
            return $"{nameof(Body)}: {Body}, {nameof(Topic)}: {Topic}, {nameof(ConsumedAt)}: {ConsumedAt}, {nameof(Id)}: {Id}";
        }
    }
}