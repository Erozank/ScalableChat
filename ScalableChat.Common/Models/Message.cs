using Cassandra;

namespace ScalableChat.Common.Models
{
    public class Message
    {
        public required string Id { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required Guid SenderId { get; set; }
        public required Guid ChatId { get; set; }
        public required string Content { get; set; }
    }
}