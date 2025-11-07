using StackExchange.Redis;

namespace ScalableChat.SignalR.Services
{
    public class PresenceService(IConnectionMultiplexer redis, ILogger<PresenceService> logger) : IPresenceService
    {
        private readonly IDatabase db = redis.GetDatabase();
        private readonly ILogger<PresenceService> logger = logger;


        public async Task UpdatePresenceAsync(string userId, string connectionId, string serverId)
        {
            await db.StringSetAsync($"connection:{connectionId}:presence", serverId, TimeSpan.FromSeconds(60));
            await db.SetAddAsync($"user:{userId}:connections", connectionId);

            logger.LogInformation("[{User}] connection {Connection} updated -> server {Server}", userId, connectionId, serverId);
        }

        public async Task RemovePresenceAsync(string userId, string connectionId)
        {
            await db.KeyDeleteAsync($"connection:{connectionId}:presence");
            await db.SetRemoveAsync($"user:{userId}:connections", connectionId);

            logger.LogInformation("[{User}] connection {Connection} removed", userId, connectionId);
        }

        public async Task<List<string>> GetUserPresenceAsync(string userId, string excludeConnection)
        {
            var connectionIds = await GetUserConnectionsAsync(userId);
            var targetConnectionIds = connectionIds.Where(x => x != excludeConnection).ToList();

            var serverIds = new List<string>();

            foreach (var connectionId in targetConnectionIds)
            {
                var serverId = await db.StringGetAsync($"connection:{connectionId}:presence");
                if (!serverId.IsNullOrEmpty)
                {
                    serverIds.Add(serverId.ToString());
                }
            }

            return serverIds.Distinct().ToList();
        }

        private async Task<List<string>> GetUserConnectionsAsync(string userId)
        {
            var connectionIds = await db.SetMembersAsync($"user:{userId}:connections");
            return connectionIds.Select(x => x.ToString()).ToList();
        }
    }
}
