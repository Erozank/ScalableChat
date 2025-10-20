using StackExchange.Redis;

namespace ScalableChat.SignalR.Services
{
    public class PresenceService(IConnectionMultiplexer redis, ILogger<PresenceService> logger) : IPresenceService
    {
        private readonly IDatabase db = redis.GetDatabase();
        private readonly ILogger<PresenceService> logger = logger;


        public async Task UpdatePresenceAsync(string userId, string serverId)
        {
            await db.StringSetAsync($"user:{userId}:presence", serverId, TimeSpan.FromMinutes(1));
            logger.LogInformation("[{User}] presence updated -> server {Server}", userId, serverId);
        }

        public async Task RemovePresenceAsync(string userId)
        {
            await db.KeyDeleteAsync($"user:{userId}:presence");
            logger.LogInformation("[{User}] presence removed", userId);
        }

        public async Task<string?> GetUserPresenceAsync(string userId)
        {
            var serverId = await db.StringGetAsync($"user:{userId}:presence");
            if (serverId.IsNullOrEmpty)
            {
                return null;
            }
            return serverId;
        }
    }
}
