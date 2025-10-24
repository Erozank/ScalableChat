using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using ScalableChat.Common.Enums;
using ScalableChat.Common.Models;
using ScalableChat.Common.Repositories;
using ScalableChat.SignalR.Models;
using ScalableChat.SignalR.Services;
using System.Text.Json;

namespace ScalableChat.SignalR.Hubs
{
    public class ChatHub(IPresenceService presenceService, IUserRepository userRepository, IFriendRequestRepository friendsRepository, IChatRepository chatRepository,
        IProducer<string, string> producer) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            await presenceService.UpdatePresenceAsync(userId!, ServerIdentity.ServerId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            await presenceService.RemovePresenceAsync(userId!);
            await base.OnDisconnectedAsync(exception);
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

            await SendKafkaMessage(userPreview, recipientUserId.Value, KafkaActionType.SendFriendRequest);
            //await Clients.User(recipientUserId.ToString()!).SendAsync("ReceiveFriendRequest", userPreview);

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
                
                var user = new UserPreview
                {
                    UserId = recipientUserId,
                    Nickname = Context.User!.Claims.First(x => x.Type == "nickname").Value
                };

                await SendKafkaMessage(user, userId, KafkaActionType.FriendRequestAccepted);
                // await Clients.User(userId.ToString()!).SendAsync("FriendRequestAccepted", user);
            }
        }

        public async Task DeleteChat(Guid chatId)
        {
            var userId = Guid.Parse(Context.UserIdentifier!);
            var friendId = await chatRepository.DeleteChatByChatId(chatId, userId);

            await SendKafkaMessage(chatId, friendId, KafkaActionType.ChatDeleted);
            // await Clients.User(friendId.ToString()!).SendAsync("ChatDeleted", chatId);
        }

        public async Task DeleteFriend(Guid userId)
        {
            var recipientUserId = Guid.Parse(Context.UserIdentifier!);
            var deleteFriendTask = friendsRepository.DeleteFriend(userId, recipientUserId);
            var deleteChatTask = chatRepository.DeleteChatByUserIds(userId, recipientUserId);

            var fiendDeletedTask = SendKafkaMessage(recipientUserId, userId, KafkaActionType.FriendDeleted);

            //var fiendDeletedTask = Clients.User(userId.ToString()!).SendAsync("FriendDeleted", recipientUserId);

            await Task.WhenAll(deleteFriendTask, deleteChatTask, fiendDeletedTask);
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
            var userId = Guid.Parse(Context.UserIdentifier!);
            var message = await chatRepository.SendMessage(userId, friendId, chatId, content);

            var sendKafkaMessageTask = SendKafkaMessage(message, friendId, KafkaActionType.SendMessage);
            
            var updateMessageTask = Clients.User(userId.ToString()!).SendAsync("UpdateMessageId", tempMessageId, message.Id);

            await Task.WhenAll(sendKafkaMessageTask, updateMessageTask);
        }

        public async Task HeartBeat()
        {
            var userId = Context.UserIdentifier;
            await presenceService.UpdatePresenceAsync(userId!, ServerIdentity.ServerId);
        }


        private async Task SendKafkaMessage<T>(T payload, Guid userId, KafkaActionType action)
        {
            var targetServerId = await presenceService.GetUserPresenceAsync(userId.ToString());
            var topic = $"delivery_to_server.{targetServerId}";

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
