
namespace ScalableChat.SignalR.Services
{
    public interface IPresenceService
    {
        Task<List<string>> GetUserPresenceAsync(string userId, string excludeConnection);
        Task RemovePresenceAsync(string userId, string connectionId);
        Task UpdatePresenceAsync(string userId, string connectionId, string serverId);
    }
}