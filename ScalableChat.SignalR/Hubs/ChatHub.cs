using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ScalableChat.Common.Enums;
using ScalableChat.Common.Models;
using ScalableChat.Common.Repositories;
using ScalableChat.SignalR.Models;
using ScalableChat.SignalR.Services;
using System.Text.Json;

namespace ScalableChat.SignalR.Hubs
{
    [Authorize]
    public class ChatHub(IPresenceService presenceService, IUserRepository userRepository, IFriendRequestRepository friendsRepository, IChatRepository chatRepository,
        IProducer<string, string> producer) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;
            await presenceService.UpdatePresenceAsync(userId!, connectionId, ServerIdentity.ServerId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;
            await presenceService.RemovePresenceAsync(userId!, connectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> SendFriendRequest(string nickname)
        {
            var connectionId = Context.ConnectionId;
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

            await SendKafkaMessage(userPreview, recipientUserId.Value, KafkaActionType.SendFriendRequest, connectionId);

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

        public async Task AcceptFriendRequest(Guid friendId)
        {
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.UserIdentifier!);
            var friendshipStatus = await friendsRepository.GetFriendshipStatus(friendId, userId);
            if (friendshipStatus == FriendshipStatus.Pending)
            {
                await friendsRepository.AcceptFriendRequestAsync(friendId, userId);
                
                var user = new UserPreview
                {
                    UserId = userId,
                    Nickname = Context.User!.Claims.First(x => x.Type == "nickname").Value
                };

                await SendKafkaMessage(user, friendId, KafkaActionType.FriendRequestAccepted, connectionId);

                var friend = await userRepository.GetUserPreviewByUserId(friendId);
                await SendKafkaMessage(friend, userId, KafkaActionType.FriendRequestAccepted, connectionId);
            }
        }

        public async Task DeleteChat(Guid chatId)
        {
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.UserIdentifier!);
            var friendId = await chatRepository.DeleteChatByChatId(chatId, userId);

            await SendKafkaMessage(chatId, friendId, KafkaActionType.ChatDeleted, connectionId);
            await SendKafkaMessage(chatId, userId, KafkaActionType.ChatDeleted, connectionId);
        }

        public async Task DeleteFriend(Guid friendId)
        {
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.UserIdentifier!);
            var deleteFriendTask = friendsRepository.DeleteFriend(friendId, userId);
            var deleteChatTask = chatRepository.DeleteChatByUserIds(friendId, userId);

            var fiendDeletedTask = SendKafkaMessage(userId, friendId, KafkaActionType.FriendDeleted, connectionId);
            var fiendDeletedTask2 = SendKafkaMessage(friendId, userId, KafkaActionType.FriendDeleted, connectionId);

            await Task.WhenAll(deleteFriendTask, deleteChatTask, fiendDeletedTask, fiendDeletedTask2);
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

        public async Task SendMessage(Guid chatId, Guid friendId, string content, string tempMessageId)
        {
            var connectionId = Context.ConnectionId;
            var userId = Guid.Parse(Context.UserIdentifier!);
            var message = await chatRepository.SendMessage(userId, friendId, chatId, content);

            var sendKafkaMessageTask = SendKafkaMessage(message, friendId, KafkaActionType.SendMessage, connectionId);
            var sendKafkaMessageTask2 = SendKafkaMessage(message, userId, KafkaActionType.SendMessage, connectionId);
            
            var updateMessageTask = Clients.User(userId.ToString()!).SendAsync("UpdateMessageId", tempMessageId, message.Id);

            await Task.WhenAll(sendKafkaMessageTask, updateMessageTask, sendKafkaMessageTask2);
        }

        public async Task HeartBeat()
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;
            await presenceService.UpdatePresenceAsync(userId!, connectionId, ServerIdentity.ServerId);
        }


        private async Task SendKafkaMessage<T>(T payload, Guid userId, KafkaActionType action, string connectionId)
        {
            var serversIds = await presenceService.GetUserPresenceAsync(userId.ToString(), connectionId);
            foreach (var serversId in serversIds)
            {
                var topic = $"delivery_to_server.{serversId}";

                var messageValue = new KafkaMessage<T>
                {
                    Action = action,
                    Receiver = userId.ToString(),
                    Payload = payload
                };

                await producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = messageValue.Receiver,
                    Value = JsonSerializer.Serialize(messageValue)
                });
            }

        }
    }
}
