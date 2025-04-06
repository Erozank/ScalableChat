namespace ChatWithSignalR.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Nickname { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
    }
}
