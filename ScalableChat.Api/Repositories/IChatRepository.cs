using ScalableChat.Api.Models;

namespace ScalableChat.Api.Repositories
{
    public interface IChatRepository
    {
        Task<Guid> CreateDirectChatIfNotExistsAsync(Guid userA, Guid userB);
        Task<Guid> DeleteChatByChatId(Guid chatId, Guid userId);
        Task DeleteChatByUserIds(Guid userA, Guid userB);
        Task<List<ChatModel?>> GetUserChatsAsync(Guid userId);
        Task<Message> SendMessage(Guid userId, Guid friendId, Guid chatId, string content);
    }
}