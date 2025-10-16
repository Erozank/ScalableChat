namespace ScalableChat.Common.Models
{
    public class RegisterRequest
    {
        public required string Nickname { get; set; }
        public required string Password { get; set; }
    }
}
