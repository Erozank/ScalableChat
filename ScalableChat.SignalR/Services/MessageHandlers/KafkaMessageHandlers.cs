using Microsoft.AspNetCore.SignalR;
using ScalableChat.Common.Models;
using ScalableChat.SignalR.Hubs;
using ScalableChat.SignalR.Models;
using System.Text.Json;

namespace ScalableChat.SignalR.Services.MessageHandlers
{
    public class SendMessageHandler : IKafkaMessageHandler
    {
        public KafkaActionType ActionType => KafkaActionType.SendMessage;

        public async Task HandleAsync(string jsonMessage, IHubContext<ChatHub> hubContext)
        {
            var message = JsonSerializer.Deserialize<KafkaMessage<Message>>(jsonMessage);
            if (message != null)
            {
                await hubContext.Clients.User(message.Receiver).SendAsync("ReceiveMessage", message.Payload);
            }
        }
    }

    public class FriendDeletedHandler : IKafkaMessageHandler
    {
        public KafkaActionType ActionType => KafkaActionType.FriendDeleted;

        public async Task HandleAsync(string jsonMessage, IHubContext<ChatHub> hubContext)
        {
            var message = JsonSerializer.Deserialize<KafkaMessage<Guid>>(jsonMessage);
            if (message != null)
            {
                await hubContext.Clients.User(message.Receiver).SendAsync("FriendDeleted", message.Payload);
            }
        }
    }

    public class ChatDeletedHandler : IKafkaMessageHandler
    {
        public KafkaActionType ActionType => KafkaActionType.ChatDeleted;

        public async Task HandleAsync(string jsonMessage, IHubContext<ChatHub> hubContext)
        {
            var message = JsonSerializer.Deserialize<KafkaMessage<Guid>>(jsonMessage);
            if (message != null)
            {
                await hubContext.Clients.User(message.Receiver).SendAsync("ChatDeleted", message.Payload);
            }
        }
    }

    public class FriendRequestAcceptedHandler : IKafkaMessageHandler
    {
        public KafkaActionType ActionType => KafkaActionType.FriendRequestAccepted;

        public async Task HandleAsync(string jsonMessage, IHubContext<ChatHub> hubContext)
        {
            var message = JsonSerializer.Deserialize<KafkaMessage<UserPreview?>>(jsonMessage);
            if (message != null)
            {
                await hubContext.Clients.User(message.Receiver).SendAsync("FriendRequestAccepted", message.Payload);
            }
        }
    }

    public class SendFriendRequestHandler : IKafkaMessageHandler
    {
        public KafkaActionType ActionType => KafkaActionType.SendFriendRequest;

        public async Task HandleAsync(string jsonMessage, IHubContext<ChatHub> hubContext)
        {
            var message = JsonSerializer.Deserialize<KafkaMessage<UserPreview?>>(jsonMessage);
            if (message != null)
            {
                await hubContext.Clients.User(message.Receiver).SendAsync("ReceiveFriendRequest", message.Payload);
            }
        }
    }
}