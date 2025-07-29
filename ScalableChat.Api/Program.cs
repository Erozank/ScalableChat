using Cassandra;
using ScalableChat.Api.Extensions;
using ScalableChat.Api.Hubs;
using ScalableChat.Api.Infrastucture;
using ScalableChat.Api.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth();

builder.Services.AddSignalR();

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



// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.SetIsOriginAllowed((host) => true)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
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

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseCors("CorsPolicy");

app.MapHub<ChatHub>("/chat");

app.Run();
