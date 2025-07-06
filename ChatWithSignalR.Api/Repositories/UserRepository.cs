using Cassandra;
using ChatWithSignalR.Api.Entities;
using ChatWithSignalR.Api.Models;

namespace ChatWithSignalR.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Cassandra.ISession _session;
        private PreparedStatement? _insertPrep;
        private PreparedStatement? _getByEmailPrep;
        private PreparedStatement? _getUserIdByNickname;

        public UserRepository(Cassandra.ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<bool> Exists(string email)
        {
            var user = await GetByEmail(email);
            return user != null;
        }

        public async Task Insert(UserEntity user)
        {
            if (_insertPrep == null)
            {
                var statement = await _session.PrepareAsync("INSERT INTO chat_with_signalr.users (user_id, nickname, email, password_hash) VALUES (?, ?, ?, ?)");
                Interlocked.CompareExchange(ref _insertPrep, statement, null);
            }

            if (user.Id == Guid.Empty) throw new ArgumentException("User ID cannot be empty.", nameof(user));
            if (string.IsNullOrWhiteSpace(user.Nickname)) throw new ArgumentException("Nickname cannot be empty.", nameof(user));
            if (string.IsNullOrWhiteSpace(user.Email)) throw new ArgumentException("Email cannot be empty.", nameof(user));

            var boundStatement = _insertPrep!.Bind(
                user.Id,
                user.Nickname,
                user.Email,
                user.PasswordHash
            );
            await _session.ExecuteAsync(boundStatement);
        }

        public async Task<UserEntity?> GetByEmail(string email)
        {
            if (_getByEmailPrep == null)
            {
                var statement = await _session.PrepareAsync("SELECT user_id, nickname, email, password_hash FROM chat_with_signalr.users WHERE email = ?");
                Interlocked.CompareExchange(ref _getByEmailPrep, statement, null);
            }

            var boundStatement = _getByEmailPrep!.Bind(email);
            var rowSet = await _session.ExecuteAsync(boundStatement);
            var row = rowSet.FirstOrDefault();

            return row == null ? null : MapRowToUser(row);
        }

        public async Task<Guid?> GetUserIdByNickname(string nickname)
        {
            if (_getUserIdByNickname == null)
            {
                var statement = await _session.PrepareAsync("SELECT user_id FROM chat_with_signalr.users WHERE nickname = ?");
                Interlocked.CompareExchange(ref _getUserIdByNickname, statement, null);
            }

            var boundStatement = _getUserIdByNickname!.Bind(nickname);
            var rowSet = await _session.ExecuteAsync(boundStatement);
            var row = rowSet.FirstOrDefault();

            return row == null ? null : row.GetValue<Guid>("user_id");
        }

        public async Task<UserPreview?> GetUserPreviewByUserId(Guid userId)
        {
            if (_getUserIdByNickname == null)
            {
                var statement = await _session.PrepareAsync("SELECT nickname FROM chat_with_signalr.users WHERE user_id = ?");
                Interlocked.CompareExchange(ref _getUserIdByNickname, statement, null);
            }
            var boundStatement = _getUserIdByNickname!.Bind(userId);
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
                Email = row.GetValue<string>("email"),
                PasswordHash = row.GetValue<string>("password_hash")
            };
        }
    }
}
