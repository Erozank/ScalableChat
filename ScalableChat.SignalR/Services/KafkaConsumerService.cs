using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using ScalableChat.Common.Models;
using ScalableChat.SignalR.Hubs;
using ScalableChat.SignalR.Models;
using ScalableChat.SignalR.Services.MessageHandlers;
using System.Text.Json;

namespace ScalableChat.SignalR.Services
{
    public class KafkaConsumerService (ILogger<KafkaConsumerService> logger,
            IProducer<string, string> producer,
            IConsumer<string, string> consumer,
            IHubContext<ChatHub> hubContext,
            KafkaMessageHandlerService messageHandler)
        : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = $"delivery_to_server.{ServerIdentity.ServerId}";

            await CreateTopic(topic);

            consumer.Subscribe(topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));

                    if (consumeResult is null)
                    {
                        continue;
                    }

                    await messageHandler.HandleMessageAsync(consumeResult.Message.Value, hubContext);

                    logger.LogInformation("Consumed message from topic '{Topic}' at offset {Offset}",
                        consumeResult.Topic,
                        consumeResult.Offset);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error consuming message from topic '{Topic}'", topic);
                }
            }
        }

        private async Task CreateTopic(string topic)
        {
            var messageValue = new KafkaMessage<Message>
            {
                Action = KafkaActionType.Empty,
                Receiver = Guid.Empty.ToString(),
                Payload = null
            };

            await producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = "null",
                Value = JsonSerializer.Serialize(messageValue)
            });
        }
    }
}
