using Cassandra;
using ScalableChat.Api.Enums;
using ScalableChat.Api.Models;

namespace ScalableChat.Api.Repositories
{
    public class FriendRequestRepository(Cassandra.ISession session, IUserRepository userRepository) : IFriendRequestRepository
    {
        private readonly Cassandra.ISession _session = session ?? throw new ArgumentNullException(nameof(session));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private PreparedStatement? _insertPrep;
        private PreparedStatement? _getReceivedFriendRequestsPrep;
        private PreparedStatement? _getFriendshipStatusPrep;
        private PreparedStatement? _updateFriendshipStatusPrep;
        private PreparedStatement? _getFriendsPrep;

        public async Task InsertFriendRequest(Guid userId, Guid friendId)
        {
            if (_insertPrep == null)
            {
                var statement = await _session.PrepareAsync("""
                    INSERT INTO scalable_chat.friend_requests (from_user_id, to_user_id, status, requested_at) 
                    VALUES (?, ?, ?, ?)
                    """);
                Interlocked.CompareExchange(ref _insertPrep, statement, null);
            }
            var boundStatement = _insertPrep!.Bind(userId, friendId, (int)FriendshipStatus.Pending, DateTime.UtcNow);
            await _session.ExecuteAsync(boundStatement);
        }

        public async Task<IEnumerable<UserPreview>> GetReceivedFriendRequests(Guid userId)
        {
            if (_getReceivedFriendRequestsPrep == null)
            {
                var statement = await _session.PrepareAsync("""
                    SELECT from_user_id, status FROM scalable_chat.friend_requests 
                    WHERE to_user_id = ?
                    """);
                Interlocked.CompareExchange(ref _getReceivedFriendRequestsPrep, statement, null);
            }
            var boundStatement = _getReceivedFriendRequestsPrep!.Bind(userId);
            var resultSet = await _session.ExecuteAsync(boundStatement);
            var ids = resultSet.Where(x => x.GetValue<int>("status") == 0).Select(row => row.GetValue<Guid>("from_user_id"));

            return await IdsToUserPreview(ids);
        }

        public async Task<FriendshipStatus?> GetFriendshipStatus(Guid fromUserId, Guid toUserId)
        {
            if (_getFriendshipStatusPrep == null)
            {
                var statement = await _session.PrepareAsync("""
                    SELECT status FROM scalable_chat.friend_requests 
                    WHERE from_user_id = ? AND to_user_id = ?
                    """);
                Interlocked.CompareExchange(ref _getFriendshipStatusPrep, statement, null);
            }

            var boundStatement = _getFriendshipStatusPrep!.Bind(fromUserId, toUserId);
            var resultSet = await _session.ExecuteAsync(boundStatement);
            var row = resultSet.FirstOrDefault();

            if (row != null)
            {
                var statusInt = row.GetValue<int>("status");
                return (FriendshipStatus)statusInt;
            }

            return null;
        }

        public async Task UpdateFriendshipStatus(Guid fromUserId, Guid toUserId, FriendshipStatus status)
        {
            if (_updateFriendshipStatusPrep == null)
            {
                var statement = await _session.PrepareAsync("""
                    UPDATE scalable_chat.friend_requests 
                    SET status = ? 
                    WHERE from_user_id = ? AND to_user_id = ?
                    """);
                Interlocked.CompareExchange(ref _updateFriendshipStatusPrep, statement, null);
            }
            var boundStatement = _updateFriendshipStatusPrep!.Bind(status, fromUserId, toUserId);
            await _session.ExecuteAsync(boundStatement);
        }

        public async Task AcceptFriendRequestAsync(Guid fromUserId, Guid toUserId)
        {
            var now = DateTimeOffset.UtcNow;

            var batch = new BatchStatement()
                .Add(new SimpleStatement(@"
                    UPDATE scalable_chat.friend_requests
                    SET status = ?, responded_at = ?
                    WHERE from_user_id = ? AND to_user_id = ?",
                    1, now, fromUserId, toUserId
                ))
                .Add(new SimpleStatement(@"
                    INSERT INTO scalable_chat.user_friends (user_id, friend_id, since)
                    VALUES (?, ?, ?)",
                    fromUserId, toUserId, now
                ))
                .Add(new SimpleStatement(@"
                    INSERT INTO scalable_chat.user_friends (user_id, friend_id, since)
                    VALUES (?, ?, ?)",
                    toUserId, fromUserId, now
                ));

            await _session.ExecuteAsync(batch);
        }

        public async Task<IEnumerable<UserPreview>> GetFriendsAsync(Guid userId)
        {
            if (_getFriendsPrep == null)
            {
                var statement = await _session.PrepareAsync("""
                    SELECT friend_id FROM scalable_chat.user_friends 
                    WHERE user_id = ?
                    """);
                Interlocked.CompareExchange(ref _getFriendsPrep, statement, null);
            }
            var boundStatement = _getFriendsPrep!.Bind(userId);
            var resultSet = await _session.ExecuteAsync(boundStatement);

            var friendsIds = resultSet.Select(row => row.GetValue<Guid>("friend_id")).ToList();

            return await IdsToUserPreview(friendsIds);
        }

        private async Task<IEnumerable<UserPreview>> IdsToUserPreview(IEnumerable<Guid> ids)
        {
            var userPreviews = new List<UserPreview>();

            foreach (var id in ids)
            {
                var friend = await _userRepository.GetUserPreviewByUserId(id);
                if (friend != null)
                {
                    userPreviews.Add(friend);
                }
            }

            return userPreviews;
        }
    }
}
