using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using ScalableChat.Common.Models;
using ScalableChat.SignalR.Hubs;
using ScalableChat.SignalR.Models;
using System.Text.Json;

namespace ScalableChat.SignalR.Services
{
    public class KafkaConsumerService(ILogger<KafkaConsumerService> logger, IProducer<string, string> producer, IConsumer<string, string> consumer, IHubContext<ChatHub> hubContext) : BackgroundService
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

                    if(consumeResult is null)
                    {
                        continue;
                    }

                    var kafkaMessage = JsonSerializer.Deserialize<KafkaMessage<Message>>(consumeResult.Message.Value);

                    if (kafkaMessage is not null)
                    {
                        await hubContext.Clients.User(kafkaMessage.Receiver).SendAsync("ReceiveMessage", kafkaMessage.Payload);
                        
                        logger.LogInformation("Consumed message '{Message}' from topic '{Topic}' at offset {Offset}",
                        consumeResult.Message.Value,
                        consumeResult.Topic,
                        consumeResult.Offset);
                    }
                    
                    await Task.Delay(5000);
                }
                catch (Exception)
                {
                    logger.LogError("Error consuming message from topic '{Topic}'", topic);
                }
            }
        }

        private async Task CreateTopic(string topic)
        {
            var messageValue = new KafkaMessage<Message>
            {
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
