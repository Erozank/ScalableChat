
using ScalableChat.Api.Enums;
using ScalableChat.Api.Models;

namespace ScalableChat.Api.Repositories
{
    public interface IFriendRequestRepository
    {
        Task AcceptFriendRequestAsync(Guid fromUserId, Guid toUserId);
        Task<IEnumerable<UserPreview>> GetFriendsAsync(Guid userId);
        Task<FriendshipStatus?> GetFriendshipStatus(Guid userId, Guid friendId);
        Task<IEnumerable<UserPreview>> GetReceivedFriendRequests(Guid userId);
        Task InsertFriendRequest(Guid userId, Guid friendId);
        Task UpdateFriendshipStatus(Guid userId, Guid friendId, FriendshipStatus status);
    }
}