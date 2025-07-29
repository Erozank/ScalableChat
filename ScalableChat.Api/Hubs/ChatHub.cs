using Microsoft.AspNetCore.SignalR;
using ScalableChat.Api.Enums;
using ScalableChat.Api.Models;
using ScalableChat.Api.Repositories;

namespace ScalableChat.Api.Hubs
{
    public class ChatHub(IUserRepository userRepository, IFriendRequestRepository friendsRepository) : Hub
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly IFriendRequestRepository friendsRepository = friendsRepository;

        public async Task<bool> SendFriendRequest(string nickname)
        {
            var senderUserId = Guid.Parse(Context.UserIdentifier!);
            var senderNickname = Context.User!.Claims.First(x => x.Type == "nickname").Value;
            var userPreview = new UserPreview
            {
                UserId = senderUserId,
                Nickname = senderNickname
            };

            var recipientUserId = await userRepository.GetUserIdByNickname(nickname);
            if (recipientUserId == null || recipientUserId.Value == senderUserId)
            {
                return false; // User not found
            }

            await Clients.User(recipientUserId.ToString()!).SendAsync("ReceiveFriendRequest", userPreview);

            var friendshipStatus = await friendsRepository.GetFriendshipStatus(senderUserId, recipientUserId.Value);
            
            if (friendshipStatus is FriendshipStatus.Pending or FriendshipStatus.Accepted)
            {
                // If the user has already sent a request or the request is accepted, we do not send it again
            }
            else if (friendshipStatus == FriendshipStatus.Rejected)
            {
                // If the user has previously rejected the request, we can re-send it
                await friendsRepository.UpdateFriendshipStatus(senderUserId, recipientUserId.Value, FriendshipStatus.Pending);
            }
            else
            {
                // If no previous friendship status exists, we insert a new friend request
                await friendsRepository.InsertFriendRequest(senderUserId, recipientUserId.Value);
            }

            return true;
        }

        public async Task AcceptFriendRequest(Guid userId)
        {
            var recipientUserId = Guid.Parse(Context.UserIdentifier!);
            var friendshipStatus = await friendsRepository.GetFriendshipStatus(userId, recipientUserId);
            if (friendshipStatus == FriendshipStatus.Pending)
            {
                await friendsRepository.AcceptFriendRequestAsync(userId, recipientUserId);
                await Clients.User(userId.ToString()!).SendAsync("FriendRequestAccepted", userId);
            }
        }

        public async Task RejectFriendrequest(Guid userId)
        {
            var recipientUserId = Guid.Parse(Context.UserIdentifier!);
            var friendshipStatus = await friendsRepository.GetFriendshipStatus(userId, recipientUserId);
            if (friendshipStatus == FriendshipStatus.Pending)
            {
                await friendsRepository.UpdateFriendshipStatus(userId, recipientUserId, FriendshipStatus.Rejected);
            }
        }
    }
}
