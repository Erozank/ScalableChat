
namespace ScalableChat.SignalR.Services
{
    public interface IPresenceService
    {
        Task<string?> GetUserPresenceAsync(string userId);
        Task RemovePresenceAsync(string userId);
        Task UpdatePresenceAsync(string userId, string serverId);
    }
}