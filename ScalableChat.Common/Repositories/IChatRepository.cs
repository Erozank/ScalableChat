using ScalableChat.Common.Models;

namespace ScalableChat.Common.Repositories
{
    public interface IChatRepository
    {
        Task<Guid> CreateDirectChatIfNotExistsAsync(Guid userA, Guid userB);
        Task<Guid> DeleteChatByChatId(Guid chatId, Guid userId);
        Task DeleteChatByUserIds(Guid userA, Guid userB);
        Task<UserPreview?> GetDirectChatInfo(Guid userId, Guid chatId);
        Task<List<ChatModel?>> GetUserChatsAsync(Guid userId);
        Task<Message> SendMessage(Guid userId, Guid friendId, Guid chatId, string content);
    }
}