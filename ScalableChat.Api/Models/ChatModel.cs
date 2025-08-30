namespace ScalableChat.Api.Models
{
    public class ChatModel
    {
        public required Guid ChatId { get; set; }
        public required UserPreview Friend { get; set; }
        public required List<Message> Messages { get; set; } = new List<Message>();
    }
}
