using Microsoft.AspNetCore.SignalR;
using ScalableChat.SignalR.Hubs;
using ScalableChat.SignalR.Models;

namespace ScalableChat.SignalR.Services.MessageHandlers
{
    public interface IKafkaMessageHandler
    {
        KafkaActionType ActionType { get; }
        Task HandleAsync(string jsonMessage, IHubContext<ChatHub> hubContext);
    }
}