namespace ScalableChat.Api.Models
{
    public class RegisterRequest
    {
        public required string Nickname { get; set; }
        public required string Password { get; set; }
    }
}
