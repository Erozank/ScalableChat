namespace ScalableChat.SignalR.Models
{
    public interface IKafkaMessage
    {
        KafkaActionType Action { get; set; }
        string Receiver { get; set; }
    }

    public class KafkaMessage<T> : IKafkaMessage
    {
        public required KafkaActionType Action { get; set; }
        public required string Receiver { get; set; }
        public required T? Payload { get; set; }
    }
}
