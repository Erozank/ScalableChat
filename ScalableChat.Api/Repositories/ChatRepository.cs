using Cassandra;
using ScalableChat.Api.Models;
using System.Linq;

namespace ScalableChat.Api.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly Cassandra.ISession _session;
        private readonly IUserRepository _userRepository;

        public ChatRepository(Cassandra.ISession session, IUserRepository userRepository)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _userRepository = userRepository;
        }

        // Get Direct Chat
        private async Task<Guid?> GetDirectChatAsync(Guid userA, Guid userB)
        {
            var stmt = await _session.PrepareAsync("""
            SELECT chat_id FROM scalable_chat.direct_chats
            WHERE user_a = ? AND user_b = ?
            """);
            var bound = stmt.Bind(userA, userB);
            var resultSet = await _session.ExecuteAsync(bound);
            var row = resultSet.FirstOrDefault();
            return row?.GetValue<Guid>("chat_id");
        }

        public async Task<Guid> CreateDirectChatIfNotExistsAsync(Guid userA, Guid userB)
        {
            if (userA.CompareTo(userB) > 0)
            {
                (userA, userB) = (userB, userA);
            }

            var chatId = await GetDirectChatAsync(userA, userB);
            if (chatId.HasValue)
            {
                return chatId.Value; // Chat already exists
            }

            chatId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var batch = new BatchStatement();

            batch.Add(new SimpleStatement("""
            INSERT INTO scalable_chat.direct_chats (user_a, user_b, chat_id)
            VALUES (?, ?, ?)
            """, userA, userB, chatId));

            batch.Add(new SimpleStatement("""
            INSERT INTO scalable_chat.direct_chats_by_chat_id (user_a, user_b, chat_id)
            VALUES (?, ?, ?)
            """, userA, userB, chatId));

            batch.Add(new SimpleStatement("""
            INSERT INTO scalable_chat.user_chats (user_id, last_message_at, chat_id)
            VALUES (?, ?, ?)
            """, userA, now, chatId));

            batch.Add(new SimpleStatement("""
            INSERT INTO scalable_chat.user_chats (user_id, last_message_at, chat_id)
            VALUES (?, ?, ?)
            """, userB, now, chatId));

            await _session.ExecuteAsync(batch);

            return chatId.Value;
        }

        // Get User Chats
        public async Task<List<ChatModel?>> GetUserChatsAsync(Guid userId)
        {
            var stmt = await _session.PrepareAsync("""
            SELECT chat_id FROM scalable_chat.user_chats
            WHERE user_id = ?
            """);
            var bound = stmt.Bind(userId);
            var resultSet = await _session.ExecuteAsync(bound);
            var chatIds = resultSet.Select(row => row.GetValue<Guid>("chat_id"));
            
            var chats = new List<ChatModel?>();
            var tasks = new List<Task<ChatModel?>>();

            tasks.AddRange(chatIds.Select(chatId => GetChatAsync(chatId, userId)));
            chats.AddRange(await Task.WhenAll(tasks));

            return chats;
        }

        private async Task<ChatModel?> GetChatAsync(Guid chatId, Guid userId)
        {
            var stmt = await _session.PrepareAsync("""
            SELECT user_a, user_b FROM scalable_chat.direct_chats_by_chat_id
            WHERE chat_id = ?
            """);
            var bound = stmt.Bind(chatId);
            var resultSet = await _session.ExecuteAsync(bound);
            var row = resultSet.FirstOrDefault();
            if (row == null)
            {
                return null; // Chat not found
            }
            var friendId = row.GetValue<Guid>("user_a") == userId ? row.GetValue<Guid>("user_b") : row.GetValue<Guid>("user_a");
            var friend = await _userRepository.GetUserPreviewByUserId(friendId);
            var messages = await GetMessagesAsync(chatId);

            var chatInfo = new ChatModel
            {
                ChatId = chatId,
                Friend = friend,
                Messages = messages.ToList(),
            };
            return chatInfo;
        }

        // Get messages in a chat
        private async Task<IEnumerable<Message>> GetMessagesAsync(Guid chatId, int limit = 50)
        {
            var stmt = await _session.PrepareAsync("""
            SELECT message_id, sender_id, content, created_at FROM scalable_chat.messages
            WHERE chat_id = ?
            ORDER BY message_id DESC
            LIMIT ?
            """);
            var bound = stmt.Bind(chatId, limit);
            var resultSet = await _session.ExecuteAsync(bound);
            return resultSet.Select(row => new Message
            {
                Id = row.GetValue<Guid>("message_id").ToString(),
                SenderId = row.GetValue<Guid>("sender_id"),
                Content = row.GetValue<string>("content"),
                ChatId = chatId,
                CreatedAt = row.GetValue<DateTime>("created_at")
            });
        }

        public async Task<Message> SendMessage(Guid userId, Guid friendId, Guid chatId, string content)
        {
            var messageId = TimeUuid.NewId();
            var now = DateTime.UtcNow;
            var batch = new BatchStatement();
            batch.Add(new SimpleStatement("""
            INSERT INTO scalable_chat.messages (message_id, chat_id, sender_id, content, created_at)
            VALUES (?, ?, ?, ?, ?)
            """, messageId, chatId, userId, content, now));
            
            batch.Add(new SimpleStatement("""
            INSERT INTO scalable_chat.user_chats (user_id, last_message_at, chat_id)
            VALUES (?, ?, ?)
            """, userId, now, chatId));

            batch.Add(new SimpleStatement("""
            INSERT INTO scalable_chat.user_chats (user_id, last_message_at, chat_id)
            VALUES (?, ?, ?)
            """, friendId, now, chatId));
            await _session.ExecuteAsync(batch);

            return new Message
            {
                Id = messageId.ToString(),
                SenderId = userId,
                Content = content,
                ChatId = chatId,
                CreatedAt = now
            };
        }

        public async Task DeleteChatByUserIds(Guid userA, Guid userB)
        {
            if (userA.CompareTo(userB) > 0)
            {
                (userA, userB) = (userB, userA);
            }

            var chatId = await GetDirectChatAsync(userA, userB);
            if (!chatId.HasValue)
            {
                return; // Chat doesn't exist
            }

            var batch = new BatchStatement();
            batch.Add(new SimpleStatement("""
                DELETE FROM scalable_chat.direct_chats WHERE user_a = ? AND user_b = ?
                """, userA, userB));
            batch.Add(new SimpleStatement("""
                DELETE FROM scalable_chat.direct_chats WHERE user_a = ? AND user_b = ?
                """, userB, userA));
            batch.Add(new SimpleStatement("""
                DELETE FROM scalable_chat.direct_chats_by_chat_id WHERE chat_id = ?
                """, chatId));
            batch.Add(new SimpleStatement("""
                DELETE FROM scalable_chat.user_chats WHERE user_id = ? AND chat_id = ?
                """, userA, chatId));
            batch.Add(new SimpleStatement("""
                DELETE FROM scalable_chat.user_chats WHERE user_id = ? AND chat_id = ?
                """, userB, chatId));
            batch.Add(new SimpleStatement("""
                DELETE FROM scalable_chat.messages WHERE chat_id = ?
                """, chatId));

            await _session.ExecuteAsync(batch);
        }

        public async Task<Guid> DeleteChatByChatId(Guid chatId, Guid userId)
        {
            var usersIdsInChat = await GetUserIdsInChat(chatId);

            // Check if the user is part of the chat
            if (usersIdsInChat.Any(id => id == userId))
            {
                var friendId = usersIdsInChat.First(id => id != userId);
                await DeleteChatByUserIds(userId, friendId);
                return friendId;
            }

            return Guid.Empty; // User is not part of the chat
        }

        private async Task<IEnumerable<Guid>> GetUserIdsInChat(Guid chatId)
        {
            var stmt = await _session.PrepareAsync("""
            SELECT user_a, user_b FROM scalable_chat.direct_chats_by_chat_id
            WHERE chat_id = ?
            """);
            var bound = stmt.Bind(chatId);
            var resultSet = await _session.ExecuteAsync(bound);
            var row = resultSet.FirstOrDefault();
            if (row == null)
            {
                return Enumerable.Empty<Guid>(); // Chat not found
            }
            return new[] { row.GetValue<Guid>("user_a"), row.GetValue<Guid>("user_b") };
        }

    }
}
