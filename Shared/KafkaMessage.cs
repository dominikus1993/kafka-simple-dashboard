namespace KafkaSimpleDashboard.Shared
{
    public class KafkaMessage
    {
        public string Topic { get; set; }
        public string Body { get; set; }

        public override string ToString()
        {
            return $"{nameof(Topic)}: {Topic}, {nameof(Body)}: {Body}";
        }
    }
}