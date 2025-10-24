using Arshid.Aspire.ApiDocs.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var kafka = builder.AddKafka("kafka")
    .WithKafkaUI();

var chatApi = builder.AddProject<Projects.ScalableChat_Api>("chat-api")
    .WithSwagger();

var chatSignalR = builder.AddProject<Projects.ScalableChat_SignalR>("chat-signalr")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(kafka)
    .WaitFor(kafka)
    //.WithReplicas(2)
    ;

builder.AddNpmApp("react", "../ScalableChat.Ui", "dev")
    .WithReference(chatApi)
    .WaitFor(chatApi)
    .WithReference(chatSignalR)
    .WaitFor(chatSignalR)
    .WithEnvironment("BROWSER", "none") // Disable opening browser on npm start
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
