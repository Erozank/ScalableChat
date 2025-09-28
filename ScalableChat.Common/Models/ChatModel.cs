namespace ScalableChat.Common.Models
{
    public class ChatModel
    {
        public required Guid ChatId { get; set; }
        public required UserPreview Friend { get; set; }
        public required List<Message> Messages { get; set; } = [];
    }
}
