namespace ScalableChat.Common.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public required string Nickname { get; set; }
        public required string PasswordHash { get; set; }
    }
}
