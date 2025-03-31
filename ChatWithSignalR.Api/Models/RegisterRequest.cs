namespace ChatWithSignalR.Api.Models
{
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Nickname { get; set; }
    }
}
