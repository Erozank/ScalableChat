using Arshid.Aspire.ApiDocs.Extensions;

var builder = DistributedApplication.CreateBuilder(args);


var chatApi = builder.AddProject<Projects.ScalableChat_Api>("chat-api")
    .WithSwagger();

var react = builder.AddNpmApp("react", "../ScalableChat.Ui", "dev")
    .WithReference(chatApi)
    .WaitFor(chatApi)
    .WithEnvironment("BROWSER", "none") // Disable opening browser on npm start
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

chatApi.WithReference(react);

builder.Build().Run();
