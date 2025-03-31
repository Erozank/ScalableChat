using Arshid.Aspire.ApiDocs.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

if (builder.ExecutionContext.IsRunMode)
{
    // Data volumes don't work on ACA for Postgres so only add when running
    postgres.WithDataVolume();
}

var postgresdb = postgres.AddDatabase("postgresdb");


var chatApi = builder.AddProject<Projects.ChatWithSignalR_Api>("chat-api")
    .WithSwagger()
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

var react = builder.AddNpmApp("react", "../ChatWithSignalR.Ui", "dev")
    .WithReference(chatApi)
    .WaitFor(chatApi)
    .WithEnvironment("BROWSER", "none") // Disable opening browser on npm start
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
