namespace ScalableChat.SignalR.Services
{
    public static class ServerIdentity
    {
        public static readonly string ServerId = Environment.GetEnvironmentVariable("SERVER_ID") ?? $"chat-{Guid.NewGuid().ToString()[..8]}";
    }
}
