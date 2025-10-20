namespace ScalableChat.SignalR.Models
{
    public class KafkaMessage<T>
    {
        public required string Receiver { get; set; }
        public required T? Payload { get; set; }
    }
}
