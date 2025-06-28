using Cassandra;
using ChatWithSignalR.Api.Enums;

namespace ChatWithSignalR.Api.Repositories
{
    public class FriendsRepository
    {
        private readonly Cassandra.ISession _session;
        private PreparedStatement? _insertPrep;
        private PreparedStatement? _getReceivedFriendRequestsPrep;

        public FriendsRepository(Cassandra.ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task InsertFriendRequest(Guid userId, Guid friendId)
        {
            if (_insertPrep == null)
            {
                var statement = await _session.PrepareAsync("""
                    INSERT INTO chat_with_signalr.friend_requests (from_user_id, to_user_id, status, requested_at) 
                    VALUES (?, ?, ?, ?)
                    """);
                Interlocked.CompareExchange(ref _insertPrep, statement, null);
            }
            var boundStatement = _insertPrep!.Bind(userId, friendId, FriendshipStatus.Pending, DateTime.UtcNow);
            await _session.ExecuteAsync(boundStatement);
        }

        public async Task<IEnumerable<Guid>> GetReceivedFriendRequests(Guid userId)
        {
            if (_getReceivedFriendRequestsPrep == null)
            {
                var statement = await _session.PrepareAsync("""
                    SELECT from_user_id FROM chat_with_signalr.friend_requests 
                    WHERE to_user_id = ? AND status = ?
                    """);
                Interlocked.CompareExchange(ref _getReceivedFriendRequestsPrep, statement, null);
            }
            var boundStatement = _getReceivedFriendRequestsPrep!.Bind(userId, FriendshipStatus.Pending);
            var resultSet = await _session.ExecuteAsync(boundStatement);
            return resultSet.Select(row => row.GetValue<Guid>("from_user_id"));
        }
    }
}
