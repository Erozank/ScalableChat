using Cassandra;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ScalableChat.Common.Infrastucture;
using ScalableChat.Common.Repositories;
using ScalableChat.SignalR.Hubs;
using ScalableChat.SignalR.Services;
using ScalableChat.SignalR.Services.MessageHandlers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add SignalR
builder.Services.AddSignalR();

// Register Kafka message handlers
builder.Services.AddSingleton<IKafkaMessageHandler, SendMessageHandler>();
builder.Services.AddSingleton<IKafkaMessageHandler, FriendDeletedHandler>();
builder.Services.AddSingleton<IKafkaMessageHandler, ChatDeletedHandler>();
builder.Services.AddSingleton<IKafkaMessageHandler, FriendRequestAcceptedHandler>();
builder.Services.AddSingleton<IKafkaMessageHandler, SendFriendRequestHandler>();
builder.Services.AddSingleton<KafkaMessageHandlerService>();

builder.AddRedisClient(connectionName: "cache");

builder.AddKafkaProducer<string, string>("kafka");
builder.AddKafkaConsumer<string, string>("kafka", options =>
{
    options.Config.GroupId = "consumer-group";
    options.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddSingleton<ICluster>(sp => {
    return Cluster.Builder().AddContactPoint("localhost")
                    .WithPort(9042)
                    .Build();
});
builder.Services.AddSingleton<Cassandra.ISession>(sp => {
    var cluster = sp.GetRequiredService<ICluster>();
    try
    {
        return cluster.Connect();
    }
    catch (Exception ex)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Failed to connect to ScyllaDB Keyspace scalable_chat");
        throw;
    }
});
builder.Services.AddSingleton<IPresenceService, PresenceService>();

builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<ChatHub>("/chathub");

app.Run();



