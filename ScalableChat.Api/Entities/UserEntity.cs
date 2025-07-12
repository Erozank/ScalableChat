namespace ScalableChat.Api.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public required string Nickname { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
    }
}
