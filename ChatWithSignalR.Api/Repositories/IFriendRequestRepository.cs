
using ChatWithSignalR.Api.Enums;
using ChatWithSignalR.Api.Models;

namespace ChatWithSignalR.Api.Repositories
{
    public interface IFriendRequestRepository
    {
        Task AcceptFriendRequestAsync(Guid fromUserId, Guid toUserId);
        Task<IEnumerable<UserPreview>> GetFriendsAsync(Guid userId);
        Task<FriendshipStatus?> GetFriendshipStatus(Guid userId, Guid friendId);
        Task<IEnumerable<Guid>> GetReceivedFriendRequests(Guid userId);
        Task InsertFriendRequest(Guid userId, Guid friendId);
        Task UpdateFriendshipStatus(Guid userId, Guid friendId, FriendshipStatus status);
    }
}