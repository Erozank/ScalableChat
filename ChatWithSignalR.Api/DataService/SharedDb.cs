using ChatWithSignalR.Api.Models;
using System.Collections.Concurrent;

namespace ChatWithSignalR.Api.DataService
{
    public class SharedDb
    {
        private readonly ConcurrentDictionary<string, UserConnection> connections = new ConcurrentDictionary<string, UserConnection>();

        public ConcurrentDictionary<string, UserConnection> Connections => connections;
    }
}
