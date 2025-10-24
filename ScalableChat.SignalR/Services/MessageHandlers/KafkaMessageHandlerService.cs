using System.Text.Json.Nodes;
using Microsoft.AspNetCore.SignalR;
using ScalableChat.SignalR.Hubs;
using ScalableChat.SignalR.Models;

namespace ScalableChat.SignalR.Services.MessageHandlers
{
    public class KafkaMessageHandlerService
    {
        private readonly ILogger<KafkaMessageHandlerService> logger;
        private readonly Dictionary<KafkaActionType, IKafkaMessageHandler> handlers;

        public KafkaMessageHandlerService(ILogger<KafkaMessageHandlerService> logger, IEnumerable<IKafkaMessageHandler> handlers)
        {
            this.logger = logger;
            this.handlers = handlers.ToDictionary(h => h.ActionType);
        }

        public async Task HandleMessageAsync(string jsonMessage, IHubContext<ChatHub> hubContext)
        {
            try
            {
                JsonNode? node = JsonNode.Parse(jsonMessage);
                if (node == null) return;

                if (Enum.TryParse<KafkaActionType>(node["Action"]?.ToString(), out var actionType) && 
                    handlers.TryGetValue(actionType, out var handler))
                {
                    await handler.HandleAsync(jsonMessage, hubContext);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling Kafka message: '{Message}'", jsonMessage);
            }
        }
    }
}