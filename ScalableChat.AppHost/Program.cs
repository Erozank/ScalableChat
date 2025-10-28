using Arshid.Aspire.ApiDocs.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var kafka = builder.AddKafka("kafka")
    .WithKafkaUI();

var scylla = builder.AddContainer("scylla-node1", "scylladb/scylla", "latest")
    .WithArgs("--seeds=scylla-node1")
    .WithVolume("scylla_node1_data", "/var/lib/scylla")
    .WithEndpoint(port: 9042, targetPort: 9042, "cql")
    .WithContainerName("scylla-node1");

builder.AddContainer("ini-scylla", "nuvo/docker-cqlsh")
    .WithBindMount("./scylla-config/init.cql", "/init.cql")
    .WithBindMount("./scylla-config/entrypoint.sh", "/entrypoint.sh")
    .WithEntrypoint("/entrypoint.sh");

var chatApi = builder.AddProject<Projects.ScalableChat_Api>("chat-api")
    .WithSwagger()
    .WaitFor(scylla);

var chatSignalR = builder.AddProject<Projects.ScalableChat_SignalR>("chat-signalr")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(kafka)
    .WaitFor(kafka)
    .WaitFor(scylla)
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
