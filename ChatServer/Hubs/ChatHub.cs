using ChatServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _bot;
        private readonly IDictionary<string, UserConnection> _connections;
        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            _bot = "My Chat";
            _connections = connections;
        }
        public async Task JoinRoom(UserConnection userConnection)
        {
            _connections.TryAdd(Context.ConnectionId, userConnection);
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _bot, $"{userConnection.User} has joined {userConnection.Room}");
            SendUsersConnected(userConnection.Room);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _bot , $"{userConnection.User} has left");
                SendUsersConnected(userConnection.Room); 
            }
        }

        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, message);
            }
        }

        public Task SendUsersConnected(string room)
        {
            var users = _connections.Values.Where(c => c.Room == room).Select(c => c.User);
            return Clients.Group(room).SendAsync("UsersInRoom", users);

        }
    }
}
