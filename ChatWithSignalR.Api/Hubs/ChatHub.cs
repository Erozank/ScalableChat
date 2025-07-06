using ChatWithSignalR.Api.DataService;
using ChatWithSignalR.Api.Enums;
using ChatWithSignalR.Api.Models;
using ChatWithSignalR.Api.Repositories;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace ChatWithSignalR.Api.Hubs
{
    public class ChatHub(SharedDb sharedDb, IUserRepository userRepository, IFriendRequestRepository friendsRepository) : Hub
    {
        private readonly SharedDb sharedDb = sharedDb;
        private readonly IUserRepository userRepository = userRepository;
        private readonly IFriendRequestRepository friendsRepository = friendsRepository;

        public async Task JoinChat(UserConnection conn)
        {
            await Clients.All.SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined");
        }

        public async Task JoinSpecificChatRoom(UserConnection conn)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.ChatRoom);

            sharedDb.Connections[Context.ConnectionId] = conn;

            await Clients.Group(conn.ChatRoom).SendAsync("JoinSpecificChatRoom", "admin", $"{conn.Username} has joined {conn.ChatRoom}");
        }

        public async Task SendMessage(string message)
        {
            if(sharedDb.Connections.TryGetValue(Context.ConnectionId, out UserConnection connection))
            {
                await Clients.Group(connection.ChatRoom).SendAsync("ReceiveMessage", connection.Username, message);
            }
        }

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
                await Clients.User(userId.ToString()!).SendAsync("FriendRequestAccepted", recipientUserId);
            }
        }

        public async Task RejectFriendrequest(Guid userId)
        {
            var recipientUserId = Guid.Parse(Context.UserIdentifier!);
            var friendshipStatus = await friendsRepository.GetFriendshipStatus(userId, recipientUserId);
            if (friendshipStatus == FriendshipStatus.Pending)
            {
                await friendsRepository.UpdateFriendshipStatus(userId, recipientUserId, FriendshipStatus.Rejected);
                await Clients.User(userId.ToString()!).SendAsync("FriendRequestAccepted", recipientUserId);
            }
        }
    }
}
