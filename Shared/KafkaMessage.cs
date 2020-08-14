namespace KafkaSimpleDashboard.Shared
{
    public class KafkaMessage
    {
        public string Body { get; set; }
        public string Topic { get; set; }

        public override string ToString()
        {
            return $"{nameof(Body)}: {Body}, {nameof(Topic)}: {Topic}";
        }
    }
}