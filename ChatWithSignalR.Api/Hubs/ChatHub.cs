using ChatWithSignalR.Api.DataService;
using ChatWithSignalR.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatWithSignalR.Api.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SharedDb sharedDb;

        public ChatHub(SharedDb sharedDb)
        {
            this.sharedDb = sharedDb;
        }

        public async Task JoinChat(UserConnection conn)
        {
            await Clients.All.SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined");
        }

        public async Task JoinSpecificChatRoom(UserConnection conn)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.ChatRoom);

            sharedDb.Connections[Context.ConnectionId] = conn;

            await Clients.Group(conn.ChatRoom).SendAsync("JoinSpecificChatRoom", "admin", $"{conn.Username} has joined {conn.ChatRoom}");
        }

        public async Task SendMessage(string message)
        {
            if(sharedDb.Connections.TryGetValue(Context.ConnectionId, out UserConnection connection))
            {
                await Clients.Group(connection.ChatRoom).SendAsync("ReceiveMessage", connection.Username, message);
            }
        }

        public async Task SendFriendRequest(string recipientUserId)
        {
            var senderUserId = Context.UserIdentifier;


            await Clients.User(recipientUserId).SendAsync("ReceiveFriendRequest", senderUserId);
        }
    }
}
