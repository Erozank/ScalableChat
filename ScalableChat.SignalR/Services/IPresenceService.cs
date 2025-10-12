
namespace ScalableChat.SignalR.Services
{
    public interface IPresenceService
    {
        Task RemovePresenceAsync(string userId);
        Task UpdatePresenceAsync(string userId, string serverId);
    }
}