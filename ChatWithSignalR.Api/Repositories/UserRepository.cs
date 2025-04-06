using Cassandra;
using ChatWithSignalR.Api.Models;

namespace ChatWithSignalR.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Cassandra.ISession _session;
        private PreparedStatement? _insertPrep;
        private PreparedStatement? _getByEmailPrep;

        public UserRepository(Cassandra.ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<bool> Exists(string email)
        {
            var user = await GetByEmail(email);
            return user != null;
        }

        public async Task Insert(User user)
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

        public async Task<User?> GetByEmail(string email)
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

        private User MapRowToUser(Row row)
        {
            return new User
            {
                Id = row.GetValue<Guid>("user_id"),
                Nickname = row.GetValue<string>("nickname"), 
                Email = row.GetValue<string>("email"),
                PasswordHash = row.GetValue<string>("password_hash")
            };
        }
    }
}
