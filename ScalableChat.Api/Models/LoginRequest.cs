namespace ScalableChat.Api.Models
{
    public class LoginRequest
    {
        public required string Nickname { get; set; }
        public required string Password { get; set; }
    }
}
