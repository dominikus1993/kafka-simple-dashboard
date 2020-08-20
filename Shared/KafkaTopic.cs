namespace KafkaSimpleDashboard.Shared
{
    public class KafkaTopic
    {
        public string Name { get; set; }
        public int Partitions { get; set; }
        public int ReplicationFactor { get; set; }
    }
}