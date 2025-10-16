using Cassandra;
using ScalableChat.Common.Entities;
using ScalableChat.Common.Models;

namespace ScalableChat.Common.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Cassandra.ISession _session;
        private PreparedStatement? _insertPrep;
        private PreparedStatement? _getByNicknamePrep;
        private PreparedStatement? _getUserIdByNickname;
        private PreparedStatement? _getNicknameByUserId;

        public UserRepository(Cassandra.ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<bool> Exists(string nickname)
        {
            var user = await GetByNickname(nickname);
            return user != null;
        }

        public async Task Insert(UserEntity user)
        {
            if (_insertPrep == null)
            {
                var statement = await _session.PrepareAsync("INSERT INTO scalable_chat.users (user_id, nickname, password_hash) VALUES (?, ?, ?)");
                Interlocked.CompareExchange(ref _insertPrep, statement, null);
            }

            if (user.Id == Guid.Empty) throw new ArgumentException("User ID cannot be empty.", nameof(user));
            if (string.IsNullOrWhiteSpace(user.Nickname)) throw new ArgumentException("Nickname cannot be empty.", nameof(user));

            var boundStatement = _insertPrep!.Bind(
                user.Id,
                user.Nickname,
                user.PasswordHash
            );
            await _session.ExecuteAsync(boundStatement);
        }

        public async Task<UserEntity?> GetByNickname(string nickname)
        {
            if (_getByNicknamePrep == null)
            {
                var statement = await _session.PrepareAsync("SELECT user_id, nickname, password_hash FROM scalable_chat.users WHERE nickname = ?");
                Interlocked.CompareExchange(ref _getByNicknamePrep, statement, null);
            }

            var boundStatement = _getByNicknamePrep!.Bind(nickname);
            var rowSet = await _session.ExecuteAsync(boundStatement);
            var row = rowSet.FirstOrDefault();

            return row == null ? null : MapRowToUser(row);
        }

        public async Task<Guid?> GetUserIdByNickname(string nickname)
        {
            if (_getUserIdByNickname == null)
            {
                var statement = await _session.PrepareAsync("SELECT user_id FROM scalable_chat.users WHERE nickname = ?");
                Interlocked.CompareExchange(ref _getUserIdByNickname, statement, null);
            }

            var boundStatement = _getUserIdByNickname!.Bind(nickname);
            var rowSet = await _session.ExecuteAsync(boundStatement);
            var row = rowSet.FirstOrDefault();

            return row == null ? null : row.GetValue<Guid>("user_id");
        }

        public async Task<UserPreview?> GetUserPreviewByUserId(Guid userId)
        {
            if (_getNicknameByUserId == null)
            {
                var statement = await _session.PrepareAsync("SELECT nickname FROM scalable_chat.users WHERE user_id = ?");
                Interlocked.CompareExchange(ref _getNicknameByUserId, statement, null);
            }
            var boundStatement = _getNicknameByUserId!.Bind(userId);
            var rowSet = await _session.ExecuteAsync(boundStatement);
            var row = rowSet.FirstOrDefault();

            return row == null ? null : new UserPreview
            {
                UserId = userId,
                Nickname = row.GetValue<string>("nickname")
            };
        }


        private UserEntity MapRowToUser(Row row)
        {
            return new UserEntity
            {
                Id = row.GetValue<Guid>("user_id"),
                Nickname = row.GetValue<string>("nickname"), 
                PasswordHash = row.GetValue<string>("password_hash")
            };
        }
    }
}
