namespace ScalableChat.Api.Models
{
    public class UserPreview
    {
        public Guid UserId { get; set; }
        public required string Nickname { get; set; }
    }
}
